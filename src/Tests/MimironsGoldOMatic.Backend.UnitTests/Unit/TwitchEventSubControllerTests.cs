using System.Text;
using System.Text.Json;
using MimironsGoldOMatic.Backend.Configuration;
using MimironsGoldOMatic.Backend.Api.Controllers;
using MimironsGoldOMatic.Backend.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MimironsGoldOMatic.Backend.UnitTests.Unit;

/// <summary>EventSub webhook: challenge echo, subscription filter, chat message fan-out.</summary>
[Trait("Category", "Unit")]
public sealed class TwitchEventSubControllerTests
{
    [Fact]
    public async Task Should_ReturnChallengePlainText_WhenPayloadContainsChallenge()
    {
        var ingest = new Mock<IChatEnrollmentIngest>(MockBehavior.Strict);
        var sut = CreateSut(ingest.Object, new TwitchOptions { EventSubSecret = "" });
        SetJsonBody(sut, """{"challenge":"hello-challenge"}""");

        var result = await sut.Post(CancellationToken.None);
        var content = Assert.IsType<ContentResult>(result);
        Assert.Equal("hello-challenge", content.Content);
        Assert.Equal("text/plain", content.ContentType);
        ingest.Verify(i => i.IngestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnOk_WhenSubscriptionTypeIsNotChatMessage()
    {
        var ingest = new Mock<IChatEnrollmentIngest>(MockBehavior.Strict);
        var sut = CreateSut(ingest.Object, new TwitchOptions { EventSubSecret = "" });
        SetJsonBody(sut, """{"subscription":{"type":"channel.follow"}}""");

        var result = await sut.Post(CancellationToken.None);
        Assert.IsType<OkResult>(result);
        ingest.Verify(i => i.IngestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnOk_WhenChatMessageButEventMissing()
    {
        var ingest = new Mock<IChatEnrollmentIngest>(MockBehavior.Strict);
        var sut = CreateSut(ingest.Object, new TwitchOptions { EventSubSecret = "" });
        SetJsonBody(sut, """{"subscription":{"type":"channel.chat.message"}}""");

        var result = await sut.Post(CancellationToken.None);
        Assert.IsType<OkResult>(result);
        ingest.Verify(i => i.IngestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_CallIngest_WhenChannelChatMessageWithSubscriberBadge()
    {
        var ingest = new Mock<IChatEnrollmentIngest>();
        var sut = CreateSut(ingest.Object, new TwitchOptions { EventSubSecret = "" });
        var ev = new
        {
            message_id = "m1",
            chatter_user_id = "u1",
            chatter_user_login = "login",
            message = new { text = "!twgold Abcd" },
            badges = new[] { new { set_id = "subscriber", id = "0" } },
        };
        var evJson = JsonSerializer.Serialize(ev);
        var payload = "{\"subscription\":{\"type\":\"channel.chat.message\"},\"event\":" + evJson + "}";
        SetJsonBody(sut, payload);

        var result = await sut.Post(CancellationToken.None);
        Assert.IsType<OkResult>(result);
        ingest.Verify(i => i.IngestAsync("m1", "u1", "login", "!twgold Abcd", true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_PassIsSubscriberFalse_WhenNoBadges()
    {
        var ingest = new Mock<IChatEnrollmentIngest>();
        var sut = CreateSut(ingest.Object, new TwitchOptions { EventSubSecret = "" });
        var ev = new
        {
            message_id = "m2",
            chatter_user_id = "u2",
            chatter_user_login = "x",
            message = new { text = "!twgold Abcd" },
        };
        var evJson = JsonSerializer.Serialize(ev);
        var payload = "{\"subscription\":{\"type\":\"channel.chat.message\"},\"event\":" + evJson + "}";
        SetJsonBody(sut, payload);

        await sut.Post(CancellationToken.None);
        ingest.Verify(i => i.IngestAsync("m2", "u2", "x", "!twgold Abcd", false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_WhenSecretSetAndSignatureInvalid()
    {
        var ingest = new Mock<IChatEnrollmentIngest>(MockBehavior.Strict);
        var sut = CreateSut(ingest.Object, new TwitchOptions { EventSubSecret = "webhook-secret" });
        SetJsonBody(sut, "{}");
        sut.ControllerContext.HttpContext.Request.Headers["Twitch-Eventsub-Message-Id"] = "id";
        sut.ControllerContext.HttpContext.Request.Headers["Twitch-Eventsub-Message-Timestamp"] = "t";
        sut.ControllerContext.HttpContext.Request.Headers["Twitch-Eventsub-Message-Signature"] = "sha256=deadbeef";

        var result = await sut.Post(CancellationToken.None);
        Assert.IsType<UnauthorizedResult>(result);
    }

    private static TwitchEventSubController CreateSut(IChatEnrollmentIngest ingest, TwitchOptions twitch) =>
        new(ingest, Options.Create(twitch), Mock.Of<IMediator>(), NullLogger<TwitchEventSubController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };

    private static void SetJsonBody(TwitchEventSubController sut, string json)
    {
        sut.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
    }
}
