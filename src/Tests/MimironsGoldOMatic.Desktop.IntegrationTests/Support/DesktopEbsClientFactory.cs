using MimironsGoldOMatic.Desktop.Services;
using MimironsGoldOMatic.IntegrationTesting;

namespace MimironsGoldOMatic.Desktop.IntegrationTests.Support;

internal sealed class WebApplicationHttpClientFactory : IHttpClientFactory
{
    private readonly Func<HttpClient> _create;

    public WebApplicationHttpClientFactory(Func<HttpClient> create) => _create = create;

    public HttpClient CreateClient(string name) => _create();
}

/// <summary>Builds <see cref="EbsDesktopClient"/> against a <see cref="BackendWebApplicationFactory"/> (same HTTP paths as the WPF app).</summary>
internal static class DesktopEbsClientFactory
{
    public static EbsDesktopClient Create(BackendWebApplicationFactory host, string? apiKeyOverride = null)
    {
        var key = apiKeyOverride ?? IntegrationTestConstants.DesktopApiKey;
        var factory = new WebApplicationHttpClientFactory(() => host.CreateClient());
        var abs = host.Server.BaseAddress!.AbsoluteUri;
        var baseUrl = abs.TrimEnd('/');
        return new EbsDesktopClient(factory, () => (baseUrl, key));
    }
}
