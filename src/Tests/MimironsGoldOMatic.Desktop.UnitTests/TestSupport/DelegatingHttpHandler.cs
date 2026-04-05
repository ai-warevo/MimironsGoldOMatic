using System.Net;

namespace MimironsGoldOMatic.Desktop.UnitTests.TestSupport;

/// <summary>Routes <see cref="HttpClient"/> requests to a delegate (unit tests, no network).</summary>
internal sealed class DelegatingHttpHandler : HttpMessageHandler
{
    public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? SendAsyncImpl { get; set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (SendAsyncImpl is null)
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        return SendAsyncImpl(request, cancellationToken);
    }
}

internal sealed class TestHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;

    public TestHttpClientFactory(HttpMessageHandler handler) => _handler = handler;

    /// <summary>Each <see cref="EbsDesktopClient"/> call disposes its <see cref="HttpClient"/>; keep handler alive.</summary>
    public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
}
