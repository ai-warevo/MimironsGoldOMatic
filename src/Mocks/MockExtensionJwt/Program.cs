// <!-- Created: 2026-04-05 (Tier A E2E mocks) -->
// Issues HS256 Extension JWTs compatible with MimironsGoldOMatic.Backend JwtBearer (same signing material as Twitch:ExtensionSecret or dev fallback).
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "MockExtensionJwt" }));

/// <summary>GET /token?userId=…&amp;displayName=… — Bearer token for <c>GET /api/pool/me</c>, <c>POST /api/payouts/claim</c>, etc.</summary>
app.MapGet("/token", (string userId, string? displayName, IConfiguration config) =>
{
    displayName ??= userId;
    var extensionSecretB64 = config["Twitch:ExtensionSecret"] ?? "";
    var extensionClientId = config["Twitch:ExtensionClientId"] ?? "";
    var keyBytes = ExtensionSigningKey(extensionSecretB64);
    var key = new SymmetricSecurityKey(keyBytes);
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var handler = new JwtSecurityTokenHandler();

    var claims = new List<Claim>
    {
        new("user_id", userId),
        new(ClaimTypes.Name, displayName),
    };

    var desc = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddHours(2),
        SigningCredentials = creds,
    };
    if (!string.IsNullOrEmpty(extensionClientId))
        desc.Audience = extensionClientId;

    var token = handler.CreateToken(desc);
    var jwt = handler.WriteToken(token);
    return Results.Json(new
    {
        access_token = jwt,
        token_type = "Bearer",
        expires_in = 7200,
    });
});

app.Run();

static byte[] ExtensionSigningKey(string base64Secret)
{
    if (!string.IsNullOrEmpty(base64Secret))
        return Convert.FromBase64String(base64Secret);
    return SHA256.HashData(Encoding.UTF8.GetBytes("mgm-dev-extension-secret-change-me"));
}
