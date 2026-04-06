using MimironsGoldOMatic.Desktop.Api;
using MimironsGoldOMatic.Desktop.Services;
using MimironsGoldOMatic.Desktop.Services.Updates;
using MimironsGoldOMatic.Desktop.UnitTests.TestSupport;
using MimironsGoldOMatic.Shared;
using Moq;
using Xunit;

namespace MimironsGoldOMatic.Desktop.UnitTests;

public sealed class WoWChatLogTailServiceTests
{
    [Fact]
    public async Task ProcessLineAsync_MGM_WHO_valid_json_calls_verify_once()
    {
        var spin = Guid.NewGuid();
        var json =
            $"{{\"schemaVersion\":1,\"spinCycleId\":\"{spin:D}\",\"characterName\":\"Thrall\",\"online\":true,\"capturedAt\":\"2020-01-01T00:00:00Z\"}}";
        var line = $"3/4 12:00:00 [MGM_WHO] {json}";

        var mock = new Mock<IEbsDesktopClient>(MockBehavior.Strict);
        mock.Setup(x => x.VerifyCandidateAsync(It.IsAny<VerifyCandidateRequestDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var logs = new List<string>();
        var connection = new DesktopConnectionContext { Settings = new DesktopUserSettings() };
        using var tail = new WoWChatLogTailService(connection, mock.Object, new PayoutSnapshotCache(), logs.Add);

        await tail.ProcessLineAsync(line, CancellationToken.None);

        mock.Verify(x => x.VerifyCandidateAsync(
            It.Is<VerifyCandidateRequestDto>(d => d.SpinCycleId == spin && d.CharacterName == "Thrall" && d.Online),
            It.IsAny<CancellationToken>()), Times.Once);
        Assert.Contains(logs, l => l.Contains("verify-candidate OK", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ProcessLineAsync_MGM_WHO_duplicate_line_skips_second_verify()
    {
        var spin = Guid.NewGuid();
        var json =
            $"{{\"schemaVersion\":1,\"spinCycleId\":\"{spin:D}\",\"characterName\":\"A\",\"online\":false,\"capturedAt\":\"2020-01-01T00:00:00Z\"}}";
        var line = $"[MGM_WHO] {json}";

        var mock = new Mock<IEbsDesktopClient>(MockBehavior.Strict);
        mock.Setup(x => x.VerifyCandidateAsync(It.IsAny<VerifyCandidateRequestDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var connection = new DesktopConnectionContext { Settings = new DesktopUserSettings() };
        using var tail = new WoWChatLogTailService(connection, mock.Object, new PayoutSnapshotCache(), _ => { });

        await tail.ProcessLineAsync(line, CancellationToken.None);
        await tail.ProcessLineAsync(line, CancellationToken.None);

        mock.Verify(x => x.VerifyCandidateAsync(It.IsAny<VerifyCandidateRequestDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessLineAsync_MGM_WHO_no_json_logs_skip()
    {
        var mock = new Mock<IEbsDesktopClient>(MockBehavior.Strict);
        var logs = new List<string>();
        using var tail = new WoWChatLogTailService(
            new DesktopConnectionContext { Settings = new DesktopUserSettings() },
            mock.Object,
            new PayoutSnapshotCache(),
            logs.Add);

        await tail.ProcessLineAsync("hello [MGM_WHO] no-braces", CancellationToken.None);

        mock.Verify(x => x.VerifyCandidateAsync(It.IsAny<VerifyCandidateRequestDto>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Contains(logs, l => l.Contains("parse skip", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ProcessLineAsync_MGM_WHO_invalid_json_logs_error_without_verify()
    {
        var mock = new Mock<IEbsDesktopClient>(MockBehavior.Strict);
        var logs = new List<string>();
        using var tail = new WoWChatLogTailService(
            new DesktopConnectionContext { Settings = new DesktopUserSettings() },
            mock.Object,
            new PayoutSnapshotCache(),
            logs.Add);

        await tail.ProcessLineAsync("[MGM_WHO] { not json }", CancellationToken.None);

        mock.Verify(x => x.VerifyCandidateAsync(It.IsAny<VerifyCandidateRequestDto>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Contains(logs, l => l.Contains("[MGM_WHO] JSON error", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ProcessLineAsync_MGM_ACCEPT_calls_confirm_when_cache_has_character()
    {
        var id = Guid.NewGuid();
        var cache = new PayoutSnapshotCache();
        cache.UpdateFromPending([PayoutTestData.Pending(id, characterName: "Cairne")]);

        var mock = new Mock<IEbsDesktopClient>(MockBehavior.Strict);
        mock.Setup(x => x.ConfirmAcceptanceAsync(id, "Cairne", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var logs = new List<string>();
        using var tail = new WoWChatLogTailService(
            new DesktopConnectionContext { Settings = new DesktopUserSettings() },
            mock.Object,
            cache,
            logs.Add);

        await tail.ProcessLineAsync($"[MGM_ACCEPT:{id:D}]", CancellationToken.None);

        mock.Verify(x => x.ConfirmAcceptanceAsync(id, "Cairne", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessLineAsync_MGM_ACCEPT_unknown_id_does_not_call_api()
    {
        var id = Guid.NewGuid();
        var mock = new Mock<IEbsDesktopClient>(MockBehavior.Strict);
        var logs = new List<string>();
        using var tail = new WoWChatLogTailService(
            new DesktopConnectionContext { Settings = new DesktopUserSettings() },
            mock.Object,
            new PayoutSnapshotCache(),
            logs.Add);

        await tail.ProcessLineAsync($"[MGM_ACCEPT:{id:D}]", CancellationToken.None);

        mock.Verify(x => x.ConfirmAcceptanceAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Contains(logs, l => l.Contains("unknown payout", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ProcessLineAsync_MGM_CONFIRM_patches_sent()
    {
        var id = Guid.NewGuid();
        var mock = new Mock<IEbsDesktopClient>(MockBehavior.Strict);
        mock.Setup(x => x.PatchPayoutStatusAsync(id, PayoutStatus.Sent, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var logs = new List<string>();
        using var tail = new WoWChatLogTailService(
            new DesktopConnectionContext { Settings = new DesktopUserSettings() },
            mock.Object,
            new PayoutSnapshotCache(),
            logs.Add);

        await tail.ProcessLineAsync($"[MGM_CONFIRM:{id:D}]", CancellationToken.None);

        mock.Verify(x => x.PatchPayoutStatusAsync(id, PayoutStatus.Sent, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessLineAsync_verify_failure_logs_message()
    {
        var spin = Guid.NewGuid();
        var json =
            $"{{\"schemaVersion\":1,\"spinCycleId\":\"{spin:D}\",\"characterName\":\"X\",\"online\":true,\"capturedAt\":\"2020-01-01T00:00:00Z\"}}";

        var mock = new Mock<IEbsDesktopClient>(MockBehavior.Strict);
        mock.Setup(x => x.VerifyCandidateAsync(It.IsAny<VerifyCandidateRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var logs = new List<string>();
        using var tail = new WoWChatLogTailService(
            new DesktopConnectionContext { Settings = new DesktopUserSettings() },
            mock.Object,
            new PayoutSnapshotCache(),
            logs.Add);

        await tail.ProcessLineAsync($"[MGM_WHO] {json}", CancellationToken.None);

        Assert.Contains(logs, l => l.Contains("verify-candidate failed", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ProcessLineAsync_MGM_UPDATE_CHECK_logs_up_to_date_message_when_injector_missing()
    {
        var mockApi = new Mock<IEbsDesktopClient>(MockBehavior.Strict);
        var update = new Mock<IUpdateService>(MockBehavior.Strict);
        update.Setup(x => x.CheckForUpdatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VersionCheckResult(true, false, "1.0.0", "1.0.0", null, true, "ok"));

        var logs = new List<string>();

        using var tail = new WoWChatLogTailService(
            new DesktopConnectionContext { Settings = new DesktopUserSettings() },
            mockApi.Object,
            new PayoutSnapshotCache(),
            update.Object,
            null,
            logs.Add);

        await tail.ProcessLineAsync("[MGM_UPDATE_CHECK]", CancellationToken.None);

        Assert.Contains(logs, l => l.Contains("[MGM_UPDATE_CHECK] Mimiron's Gold-o-Matic: Вы используете актуальную версию", StringComparison.Ordinal));
    }
}
