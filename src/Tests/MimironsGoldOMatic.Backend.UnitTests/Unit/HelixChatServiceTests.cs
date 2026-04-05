using System.Net;
using System.Text.Json;
using MimironsGoldOMatic.Backend.Configuration;
using MimironsGoldOMatic.Backend.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace MimironsGoldOMatic.Backend.UnitTests.Unit;

/// <summary>Helix Send Chat Message: skips when misconfigured; retries until success.</summary>
[Trait("Category", "Unit")]
public sealed class HelixChatServiceTests
{
    [Fact]
    public async Task Should_ReturnFalse_WhenBroadcasterTokenMissing()
    {
        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        var twitch = Options.Create(new TwitchOptions { BroadcasterAccessToken = "", BroadcasterUserId = "123" });
        var sut = new HelixChatService(factory.Object, twitch, NullLogger<HelixChatService>.Instance);

        var ok = await sut.TrySendRewardSentAnnouncementAsync("Hero", CancellationToken.None);
        Assert.False(ok);
    }

    [Fact]
    public async Task Should_ReturnFalse_WhenBroadcasterUserIdMissing()
    {
        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        var twitch = Options.Create(new TwitchOptions { BroadcasterAccessToken = "tok", BroadcasterUserId = "" });
        var sut = new HelixChatService(factory.Object, twitch, NullLogger<HelixChatService>.Instance);

        var ok = await sut.TrySendRewardSentAnnouncementAsync("Hero", CancellationToken.None);
        Assert.False(ok);
    }

    [Fact]
    public async Task Should_ReturnTrue_WhenHelixReturnsSuccess()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m =>
                    m.Method == HttpMethod.Post &&
                    m.RequestUri!.ToString().Contains("helix/chat/messages", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var client = new HttpClient(handler.Object);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("Helix")).Returns(client);

        var twitch = Options.Create(new TwitchOptions
        {
            BroadcasterAccessToken = "tok",
            BroadcasterUserId = "42",
            HelixClientId = "client",
        });
        var sut = new HelixChatService(factory.Object, twitch, NullLogger<HelixChatService>.Instance);

        var ok = await sut.TrySendRewardSentAnnouncementAsync("Абвг", CancellationToken.None);
        Assert.True(ok);

        handler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task Should_Retry_WhenHelixReturnsTransientFailure()
    {
        var handler = new Mock<HttpMessageHandler>();
        var calls = 0;
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(() =>
            {
                calls++;
                return Task.FromResult(calls < 2
                    ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    : new HttpResponseMessage(HttpStatusCode.OK));
            });

        var client = new HttpClient(handler.Object);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("Helix")).Returns(client);

        var twitch = Options.Create(new TwitchOptions
        {
            BroadcasterAccessToken = "tok",
            BroadcasterUserId = "42",
            HelixClientId = "c",
        });
        var sut = new HelixChatService(factory.Object, twitch, NullLogger<HelixChatService>.Instance);

        var ok = await sut.TrySendRewardSentAnnouncementAsync("Hero", CancellationToken.None);
        Assert.True(ok);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task Should_EmbedWinnerName_InJsonBody()
    {
        string? json = null;
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>(async (r, _) =>
            {
                json = r.Content != null ? await r.Content.ReadAsStringAsync() : null;
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var client = new HttpClient(handler.Object);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("Helix")).Returns(client);

        var twitch = Options.Create(new TwitchOptions
        {
            BroadcasterAccessToken = "tok",
            BroadcasterUserId = "42",
            HelixClientId = "c",
        });
        var sut = new HelixChatService(factory.Object, twitch, NullLogger<HelixChatService>.Instance);

        await sut.TrySendRewardSentAnnouncementAsync("WinnerName", CancellationToken.None);
        Assert.NotNull(json);
        using var doc = JsonDocument.Parse(json);
        var message = doc.RootElement.GetProperty("message").GetString();
        Assert.NotNull(message);
        Assert.Contains("WinnerName", message, StringComparison.Ordinal);
        Assert.Contains("Награда", message, StringComparison.Ordinal);
    }
}
