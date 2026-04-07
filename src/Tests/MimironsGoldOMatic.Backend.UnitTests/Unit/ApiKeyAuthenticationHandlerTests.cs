using System.Text.Encodings.Web;
using MimironsGoldOMatic.Backend.Infrastructure.Auth;
using MimironsGoldOMatic.Backend.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace MimironsGoldOMatic.Backend.UnitTests.Unit;

/// <summary>Desktop <c>X-MGM-ApiKey</c> scheme: no header vs mismatch vs success.</summary>
[Trait("Category", "Unit")]
public sealed class ApiKeyAuthenticationHandlerTests
{
    [Fact]
    public async Task Should_ReturnNoResult_WhenApiKeyHeaderMissing()
    {
        await using var sp = CreateProvider("configured-secret");
        var ctx = new DefaultHttpContext { RequestServices = sp };
        var auth = sp.GetRequiredService<IAuthenticationService>();
        var result = await auth.AuthenticateAsync(ctx, "ApiKey");
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Should_Fail_WhenApiKeyDoesNotMatch()
    {
        await using var sp = CreateProvider("configured-secret");
        var ctx = new DefaultHttpContext { RequestServices = sp };
        ctx.Request.Headers["X-MGM-ApiKey"] = "wrong";
        var auth = sp.GetRequiredService<IAuthenticationService>();
        var result = await auth.AuthenticateAsync(ctx, "ApiKey");
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Should_Succeed_WhenApiKeyMatches()
    {
        await using var sp = CreateProvider("configured-secret");
        var ctx = new DefaultHttpContext { RequestServices = sp };
        ctx.Request.Headers["X-MGM-ApiKey"] = "configured-secret";
        var auth = sp.GetRequiredService<IAuthenticationService>();
        var result = await auth.AuthenticateAsync(ctx, "ApiKey");
        Assert.True(result.Succeeded);
        Assert.Equal("desktop", result.Principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public async Task Should_Fail_WhenServerApiKeyNotConfigured()
    {
        await using var sp = CreateProvider(apiKey: "");
        var ctx = new DefaultHttpContext { RequestServices = sp };
        ctx.Request.Headers["X-MGM-ApiKey"] = "any";
        var auth = sp.GetRequiredService<IAuthenticationService>();
        var result = await auth.AuthenticateAsync(ctx, "ApiKey");
        Assert.False(result.Succeeded);
    }

    private static ServiceProvider CreateProvider(string apiKey)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", _ => { });
        services.AddSingleton<IOptions<MgmOptions>>(Options.Create(new MgmOptions { ApiKey = apiKey }));
        services.AddSingleton(UrlEncoder.Default);
        return services.BuildServiceProvider();
    }
}
