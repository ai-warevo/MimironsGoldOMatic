using MimironsGoldOMatic.Backend.Infrastructure.Persistence;
using MimironsGoldOMatic.Backend.Application.Roulette;
using MimironsGoldOMatic.Desktop.IntegrationTests.Support;
using MimironsGoldOMatic.Desktop.Services;
using MimironsGoldOMatic.IntegrationTesting;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MimironsGoldOMatic.Desktop.IntegrationTests;

/// <summary>Patch / confirm / error paths the Desktop triggers after chat-log events and manual actions.</summary>
[Collection(nameof(DesktopIntegrationPostgresCollection))]
[Trait("Category", "Integration")]
public sealed class DesktopPatchConfirmErrorRecoveryIntegrationTests : HttpApiFixtureBase
{
    public DesktopPatchConfirmErrorRecoveryIntegrationTests(PostgresContainerFixture pg) : base(pg)
    {
    }

    [Fact]
    public async Task Patch_unknown_payout_id_returns_http_error()
    {
        await ResetDatabaseAndRestartHostAsync();
        var ebs = DesktopEbsClientFactory.Create(Host);
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
            ebs.PatchPayoutStatusAsync(Guid.NewGuid(), PayoutStatus.InProgress, CancellationToken.None));
        Assert.Contains("404", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task After_failed_auth_valid_client_recovers()
    {
        await ResetDatabaseAndRestartHostAsync();
        var bad = DesktopEbsClientFactory.Create(Host, apiKeyOverride: "wrong");
        await Assert.ThrowsAsync<HttpRequestException>(() => bad.GetPendingAsync(CancellationToken.None));

        var id = Guid.NewGuid();
        await PayoutDocumentSeed.InsertPendingAsync(Host.Services, PayoutDocumentSeed.CreatePending(id));

        var good = DesktopEbsClientFactory.Create(Host);
        var list = await good.GetPendingAsync(CancellationToken.None);
        Assert.Contains(list, p => p.Id == id);
    }

    [Fact]
    public async Task Patch_pending_to_in_progress_then_confirm_acceptance_succeeds()
    {
        await ResetDatabaseAndRestartHostAsync();
        var id = Guid.NewGuid();
        await PayoutDocumentSeed.InsertPendingAsync(Host.Services, PayoutDocumentSeed.CreatePending(id, "Voljin"));

        var ebs = DesktopEbsClientFactory.Create(Host);
        await ebs.PatchPayoutStatusAsync(id, PayoutStatus.InProgress, CancellationToken.None);
        await ebs.ConfirmAcceptanceAsync(id, "Voljin", CancellationToken.None);
    }

    [Fact]
    public async Task VerifyCandidate_online_creates_pending_for_desktop_tail_flow()
    {
        await ResetDatabaseAndRestartHostAsync();
        await using var scope = Host.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        var spinCycleId = Guid.NewGuid();
        var winnerUid = "winner-dt-" + Guid.NewGuid().ToString("N");
        var anchor = DateTime.UtcNow;
        var cycleStart = RouletteTime.FloorToFiveMinuteUtc(anchor).AddMinutes(-5);
        var pickTime = cycleStart.AddSeconds(RouletteTime.CollectingSeconds + 1);

        await using (var s = store.LightweightSession())
        {
            s.Store(new PoolDocument
            {
                Id = EbsIds.PoolDocumentId,
                Members =
                [
                    new PoolMemberEntry
                    {
                        TwitchUserId = winnerUid,
                        TwitchDisplayName = "WD",
                        CharacterName = "Thrall",
                    },
                ],
            });
            s.Store(new SpinStateDocument
            {
                Id = EbsIds.SpinStateDocumentId,
                CycleStartUtc = cycleStart,
                SpinCycleId = spinCycleId,
                VerificationDeadlineUtc = anchor.AddHours(1),
                PoolWasEmptyAtCycleStart = false,
                PayoutCreatedForCycle = false,
            });
            await s.SaveChangesAsync();
        }

        await RouletteCycleTick.ApplyAsync(store, pickTime, CancellationToken.None);

        var ebs = DesktopEbsClientFactory.Create(Host);
        await ebs.VerifyCandidateAsync(
            new VerifyCandidateRequest(1, spinCycleId, "Thrall", true, DateTime.UtcNow),
            CancellationToken.None);

        var pending = await ebs.GetPendingAsync(CancellationToken.None);
        Assert.Contains(pending, p => p.TwitchUserId == winnerUid && p.CharacterName == "Thrall");
    }
}
