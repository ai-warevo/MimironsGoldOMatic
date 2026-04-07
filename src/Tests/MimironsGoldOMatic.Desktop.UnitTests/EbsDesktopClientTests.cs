using System.Net;
using System.Text;
using System.Text.Json;
using MimironsGoldOMatic.Desktop.Services;
using MimironsGoldOMatic.Desktop.UnitTests.TestSupport;
using Xunit;

namespace MimironsGoldOMatic.Desktop.UnitTests;

public sealed class EbsDesktopClientTests
{
    [Fact]
    public async Task GetPendingAsync_success_empty_array_returns_empty_list()
    {
        var handler = new DelegatingHttpHandler
        {
            SendAsyncImpl = (req, _) =>
            {
                Assert.Equal(HttpMethod.Get, req.Method);
                Assert.EndsWith("/api/payouts/pending", req.RequestUri!.AbsolutePath, StringComparison.Ordinal);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]", Encoding.UTF8, "application/json"),
                });
            },
        };
        var api = new EbsDesktopClient(new TestHttpClientFactory(handler), () => ("https://example/", "key"));
        var list = await api.GetPendingAsync(CancellationToken.None);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetPendingAsync_null_json_document_yields_empty_via_coalesce()
    {
        var handler = new DelegatingHttpHandler
        {
            SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json"),
            }),
        };
        var api = new EbsDesktopClient(new TestHttpClientFactory(handler), () => ("https://example/", "key"));
        var list = await api.GetPendingAsync(CancellationToken.None);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetPendingAsync_invalid_json_throws_JsonException()
    {
        var handler = new DelegatingHttpHandler
        {
            SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-json", Encoding.UTF8, "application/json"),
            }),
        };
        var api = new EbsDesktopClient(new TestHttpClientFactory(handler), () => ("https://example/", "key"));
        await Assert.ThrowsAsync<JsonException>(() => api.GetPendingAsync(CancellationToken.None));
    }

    [Fact]
    public async Task GetPendingAsync_error_status_throws_http_request_exception_with_body()
    {
        var handler = new DelegatingHttpHandler
        {
            SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway)
            {
                Content = new StringContent("upstream down", Encoding.UTF8, "text/plain"),
            }),
        };
        var api = new EbsDesktopClient(new TestHttpClientFactory(handler), () => ("https://example/", "key"));
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => api.GetPendingAsync(CancellationToken.None));
        Assert.Contains("502", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetPendingAsync_times_out_when_handler_never_completes()
    {
        var handler = new DelegatingHttpHandler
        {
            SendAsyncImpl = async (_, ct) =>
            {
                await Task.Delay(Timeout.Infinite, ct);
                return new HttpResponseMessage(HttpStatusCode.OK);
            },
        };
        var factory = new TestHttpClientFactory(handler);
        var api = new EbsDesktopClient(factory, () => ("https://example/", "key"));
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(80));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => api.GetPendingAsync(cts.Token));
    }

    [Fact]
    public void CreateClient_throws_when_base_url_missing()
    {
        var handler = new DelegatingHttpHandler { SendAsyncImpl = (_, _) => throw new InvalidOperationException("unreachable") };
        var api = new EbsDesktopClient(new TestHttpClientFactory(handler), () => ("", "key"));
        Assert.Throws<InvalidOperationException>(() => api.GetPendingAsync(CancellationToken.None).GetAwaiter().GetResult());
    }

    [Fact]
    public void CreateClient_throws_when_api_key_missing()
    {
        var handler = new DelegatingHttpHandler { SendAsyncImpl = (_, _) => throw new InvalidOperationException("unreachable") };
        var api = new EbsDesktopClient(new TestHttpClientFactory(handler), () => ("https://example/", "  "));
        Assert.Throws<InvalidOperationException>(() => api.GetPendingAsync(CancellationToken.None).GetAwaiter().GetResult());
    }

    [Fact]
    public async Task PatchPayoutStatusAsync_sends_patch_and_succeeds_on_200()
    {
        var handler = new DelegatingHttpHandler
        {
            SendAsyncImpl = (req, _) =>
            {
                Assert.Equal(HttpMethod.Patch, req.Method);
                Assert.Contains("/api/payouts/", req.RequestUri!.AbsolutePath, StringComparison.Ordinal);
                Assert.EndsWith("/status", req.RequestUri.AbsolutePath, StringComparison.Ordinal);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json"),
                });
            },
        };
        var id = Guid.NewGuid();
        var api = new EbsDesktopClient(new TestHttpClientFactory(handler), () => ("https://example/", "key"));
        await api.PatchPayoutStatusAsync(id, PayoutStatus.Sent, CancellationToken.None);
    }

    [Fact]
    public async Task ConfirmAcceptanceAsync_non_ok_throws()
    {
        var handler = new DelegatingHttpHandler
        {
            SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent("no", Encoding.UTF8, "text/plain"),
            }),
        };
        var api = new EbsDesktopClient(new TestHttpClientFactory(handler), () => ("https://example/", "key"));
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            api.ConfirmAcceptanceAsync(Guid.NewGuid(), "A", CancellationToken.None));
    }

    [Fact]
    public async Task VerifyCandidateAsync_success_on_204()
    {
        var handler = new DelegatingHttpHandler
        {
            SendAsyncImpl = (req, _) =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.EndsWith("/api/roulette/verify-candidate", req.RequestUri!.AbsolutePath, StringComparison.Ordinal);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            },
        };
        var api = new EbsDesktopClient(new TestHttpClientFactory(handler), () => ("https://example/", "key"));
        var dto = new VerifyCandidateRequest(1, Guid.NewGuid(), "X", true, DateTime.UtcNow);
        await api.VerifyCandidateAsync(dto, CancellationToken.None);
    }

    [Fact]
    public async Task GetVersionInfoAsync_success_returns_version_payload()
    {
        var handler = new DelegatingHttpHandler
        {
            SendAsyncImpl = (req, _) =>
            {
                Assert.Equal(HttpMethod.Get, req.Method);
                Assert.EndsWith("/api/version", req.RequestUri!.AbsolutePath, StringComparison.Ordinal);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"version\":\"1.2.3\"}", Encoding.UTF8, "application/json"),
                });
            },
        };
        var api = new EbsDesktopClient(new TestHttpClientFactory(handler), () => ("https://example/", "key"));

        var dto = await api.GetVersionInfoAsync(CancellationToken.None);
        Assert.Equal("1.2.3", dto.Version);
    }

    [Fact]
    public async Task GetVersionInfoAsync_missing_required_version_throws_json_exception()
    {
        var handler = new DelegatingHttpHandler
        {
            SendAsyncImpl = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),
            }),
        };
        var api = new EbsDesktopClient(new TestHttpClientFactory(handler), () => ("https://example/", "key"));

        await Assert.ThrowsAsync<JsonException>(() => api.GetVersionInfoAsync(CancellationToken.None));
    }
}
