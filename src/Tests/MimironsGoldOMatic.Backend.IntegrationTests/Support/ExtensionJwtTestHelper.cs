using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MimironsGoldOMatic.Backend.IntegrationTests.Support;

/// <summary>HS256 Extension-shaped JWTs compatible with Backend Development signing (empty <c>Twitch:ExtensionSecret</c> → dev key in <c>Program.cs</c>).</summary>
internal static class ExtensionJwtTestHelper
{
    /// <summary>Must match Development branch when <c>Twitch:ExtensionSecret</c> is unset.</summary>
    private static readonly byte[] DevSigningKey =
        SHA256.HashData(Encoding.UTF8.GetBytes("mgm-dev-extension-secret-change-me"));

    internal static string CreateViewerToken(string userId, string displayName, string extensionClientId = "test-extension-client-id")
    {
        var key = new SymmetricSecurityKey(DevSigningKey);
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
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = creds,
            Audience = extensionClientId,
        };
        return handler.WriteToken(handler.CreateToken(desc));
    }
}
