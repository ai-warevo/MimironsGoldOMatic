using System.Security.Claims;
using System.Text.Encodings.Web;
using MimironsGoldOMatic.Backend.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MimironsGoldOMatic.Backend.Auth;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly MgmOptions _mgm;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<MgmOptions> mgm)
        : base(options, logger, encoder) =>
        _mgm = mgm.Value;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-MGM-ApiKey", out var key))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (string.IsNullOrEmpty(_mgm.ApiKey) || key != _mgm.ApiKey)
            return Task.FromResult(AuthenticateResult.Fail("forbidden_apikey"));

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "desktop") };
        var id = new ClaimsIdentity(claims, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(id), Scheme.Name)));
    }
}
