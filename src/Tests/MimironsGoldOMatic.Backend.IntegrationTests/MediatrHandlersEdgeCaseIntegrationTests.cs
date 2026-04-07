using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Services;
using MimironsGoldOMatic.Backend.Common;
using MimironsGoldOMatic.Backend.IntegrationTests.Support;
using MimironsGoldOMatic.IntegrationTesting;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MimironsGoldOMatic.Backend.IntegrationTests;

[Collection(nameof(PostgresCollection))]
[Trait("Category", "Integration")]
public sealed class MediatrHandlersEdgeCaseIntegrationTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _pg;
    private ServiceProvider? _services;

    public MediatrHandlersEdgeCaseIntegrationTests(PostgresContainerFixture pg) => _pg = pg;

    public async Task InitializeAsync()
    {
        _services = BackendTestHost.CreateServiceProvider(_pg.ConnectionString);
        var store = _services.GetRequiredService<IDocumentStore>();
        await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
        await PostgresMgmTruncate.TruncateAllAsync(_pg.ConnectionString);
    }

    public async Task DisposeAsync()
    {
        if (_services != null)
            await _services.DisposeAsync();
    }

    [Fact]
    public async Task Should_ReturnIdlePhase_WhenGetRouletteStateAndNoSpinDocument()
    {
        var m = _services!.GetRequiredService<IMediator>();
        var r = await m.Send(new GetRouletteStateQuery());
        Assert.True(r.Ok);
        Assert.NotNull(r.Value);
        Assert.Equal("idle", r.Value!.SpinPhase);
        Assert.Null(r.Value.CurrentSpinCycleId);
    }

    [Fact]
    public async Task Should_ReturnNotEnrolled_WhenGetPoolMeUnknownUser()
    {
        var m = _services!.GetRequiredService<IMediator>();
        var r = await m.Send(new GetPoolMeQuery("no-such-user"));
        Assert.True(r.Ok);
        Assert.False(r.Value!.IsEnrolled);
    }

    [Fact]
    public async Task Should_Return404_WhenGetMyLastPayoutMissing()
    {
        var m = _services!.GetRequiredService<IMediator>();
        var r = await m.Send(new GetMyLastPayoutQuery("ghost"));
        Assert.True(r.Ok);
        Assert.Null(r.Value);
        Assert.Equal(404, r.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnEmptyList_WhenGetPendingPayoutsNone()
    {
        var m = _services!.GetRequiredService<IMediator>();
        var r = await m.Send(new GetPendingPayoutsQuery());
        Assert.True(r.Ok);
        Assert.NotNull(r.Value);
        Assert.Empty(r.Value!);
    }

    [Fact]
    public async Task Should_Return404_WhenConfirmAcceptanceUnknownPayout()
    {
        var m = _services!.GetRequiredService<IMediator>();
        var r = await m.Send(new ConfirmAcceptanceCommand(Guid.NewGuid(), "Abcd"));
        Assert.False(r.Ok);
        Assert.Equal(404, r.StatusCode);
    }

    [Fact]
    public async Task Should_Return400_WhenConfirmAcceptanceWrongCharacterName()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        var id = Guid.NewGuid();
        await using (var s = store.LightweightSession())
        {
            s.Store(new PayoutReadDocument
            {
                Id = id,
                TwitchUserId = "u",
                TwitchDisplayName = "d",
                CharacterName = "Abcd",
                GoldAmount = PayoutEconomics.MvpWinningPayoutGold,
                EnrollmentRequestId = "spin:x",
                Status = PayoutStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            });
            await s.SaveChangesAsync();
        }

        var m = sp.GetRequiredService<IMediator>();
        var r = await m.Send(new ConfirmAcceptanceCommand(id, "Wrong"));
        Assert.False(r.Ok);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task Should_BeIdempotent_WhenConfirmAcceptanceCalledTwice()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        var id = Guid.NewGuid();
        await using (var s = store.LightweightSession())
        {
            s.Store(new PayoutReadDocument
            {
                Id = id,
                TwitchUserId = "u",
                TwitchDisplayName = "d",
                CharacterName = "Abcd",
                GoldAmount = PayoutEconomics.MvpWinningPayoutGold,
                EnrollmentRequestId = "spin:x",
                Status = PayoutStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            });
            await s.SaveChangesAsync();
        }

        var m = sp.GetRequiredService<IMediator>();
        var r1 = await m.Send(new ConfirmAcceptanceCommand(id, "Abcd"));
        Assert.True(r1.Ok);
        var r2 = await m.Send(new ConfirmAcceptanceCommand(id, "Abcd"));
        Assert.True(r2.Ok);
    }

    [Fact]
    public async Task Should_Return400_WhenVerifyCandidateSchemaNot1()
    {
        var m = _services!.GetRequiredService<IMediator>();
        var r = await m.Send(new VerifyCandidateCommand(
            new VerifyCandidateRequest(0, Guid.NewGuid(), "Ab", true, DateTime.UtcNow)));
        Assert.False(r.Ok);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task Should_Return400_WhenVerifyCandidateInvalidCharacterName()
    {
        var m = _services!.GetRequiredService<IMediator>();
        var r = await m.Send(new VerifyCandidateCommand(
            new VerifyCandidateRequest(1, Guid.NewGuid(), "1", true, DateTime.UtcNow)));
        Assert.False(r.Ok);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task Should_Return400_WhenVerifyCandidateSpinMismatch()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        await using (var s = store.LightweightSession())
        {
            s.Store(new SpinStateDocument
            {
                Id = EbsIds.SpinStateDocumentId,
                CycleStartUtc = DateTime.UtcNow,
                SpinCycleId = Guid.NewGuid(),
                VerificationDeadlineUtc = DateTime.UtcNow.AddHours(1),
            });
            await s.SaveChangesAsync();
        }

        var m = sp.GetRequiredService<IMediator>();
        var r = await m.Send(new VerifyCandidateCommand(
            new VerifyCandidateRequest(1, Guid.NewGuid(), "Abcd", false, DateTime.UtcNow)));
        Assert.False(r.Ok);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task Should_Return400_WhenVerifyCandidatePastDeadline()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        var cycleId = Guid.NewGuid();
        await using (var s = store.LightweightSession())
        {
            s.Store(new SpinStateDocument
            {
                Id = EbsIds.SpinStateDocumentId,
                CycleStartUtc = DateTime.UtcNow.AddHours(-2),
                SpinCycleId = cycleId,
                VerificationDeadlineUtc = DateTime.UtcNow.AddMinutes(-5),
            });
            await s.SaveChangesAsync();
        }

        var m = sp.GetRequiredService<IMediator>();
        var r = await m.Send(new VerifyCandidateCommand(
            new VerifyCandidateRequest(1, cycleId, "Abcd", false, DateTime.UtcNow)));
        Assert.False(r.Ok);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task Should_Return400_WhenPostClaimInvalidCharacterName()
    {
        var m = _services!.GetRequiredService<IMediator>();
        var r = await m.Send(new PostClaimCommand("u1", "D", new CreatePayoutRequest("1bad", "e1")));
        Assert.False(r.Ok);
        Assert.Equal(400, r.StatusCode);
    }

    [Fact]
    public async Task Should_MarkCycleConsumed_WhenVerifyCandidateReportsOffline()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        var cycleId = Guid.NewGuid();
        var anchor = DateTime.UtcNow;
        await using (var s = store.LightweightSession())
        {
            s.Store(new SpinStateDocument
            {
                Id = EbsIds.SpinStateDocumentId,
                CycleStartUtc = anchor.AddMinutes(-1),
                SpinCycleId = cycleId,
                VerificationDeadlineUtc = anchor.AddHours(1),
                CandidateTwitchUserId = "u-off",
                CandidateCharacterName = "Abcd",
                PayoutCreatedForCycle = false,
            });
            await s.SaveChangesAsync();
        }

        var m = sp.GetRequiredService<IMediator>();
        var r = await m.Send(new VerifyCandidateCommand(
            new VerifyCandidateRequest(1, cycleId, "Abcd", false, DateTime.UtcNow)));
        Assert.True(r.Ok);

        await using var q = store.QuerySession();
        var spin = await q.LoadAsync<SpinStateDocument>(EbsIds.SpinStateDocumentId, CancellationToken.None);
        Assert.NotNull(spin);
        Assert.True(spin.PayoutCreatedForCycle);
    }

    [Fact]
    public async Task Should_Return409_WhenVerifyCandidateWinnerHasActivePayout()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        var cycleId = Guid.NewGuid();
        var anchor = DateTime.UtcNow;
        await using (var s = store.LightweightSession())
        {
            s.Store(new SpinStateDocument
            {
                Id = EbsIds.SpinStateDocumentId,
                CycleStartUtc = anchor.AddMinutes(-1),
                SpinCycleId = cycleId,
                VerificationDeadlineUtc = anchor.AddHours(1),
                CandidateTwitchUserId = "busy-winner",
                CandidateCharacterName = "Hero",
                CandidateTwitchDisplayName = "H",
                PayoutCreatedForCycle = false,
            });
            s.Store(new PayoutReadDocument
            {
                Id = Guid.NewGuid(),
                TwitchUserId = "busy-winner",
                TwitchDisplayName = "H",
                CharacterName = "Other",
                GoldAmount = PayoutEconomics.MvpWinningPayoutGold,
                EnrollmentRequestId = "spin:old",
                Status = PayoutStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            });
            await s.SaveChangesAsync();
        }

        var m = sp.GetRequiredService<IMediator>();
        var r = await m.Send(new VerifyCandidateCommand(
            new VerifyCandidateRequest(1, cycleId, "Hero", true, DateTime.UtcNow)));
        Assert.False(r.Ok);
        Assert.Equal(409, r.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnEnrolled_WhenGetPoolMeHasMember()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        await using (var s = store.LightweightSession())
        {
            s.Store(new PoolDocument
            {
                Id = EbsIds.PoolDocumentId,
                Members = [new PoolMemberEntry { TwitchUserId = "me-u", CharacterName = "Abcd", TwitchDisplayName = "D" }],
            });
            await s.SaveChangesAsync();
        }

        var m = sp.GetRequiredService<IMediator>();
        var r = await m.Send(new GetPoolMeQuery("me-u"));
        Assert.True(r.Ok);
        Assert.True(r.Value!.IsEnrolled);
        Assert.Equal("Abcd", r.Value.CharacterName);
    }

    [Fact]
    public async Task Should_Return404_WhenPatchPayoutUnknownId()
    {
        var m = _services!.GetRequiredService<IMediator>();
        var r = await m.Send(new PatchPayoutStatusCommand(Guid.NewGuid(), PayoutStatus.Sent));
        Assert.False(r.Ok);
        Assert.Equal(404, r.StatusCode);
    }

    [Fact]
    public async Task Should_BeIdempotent_WhenVerifyCandidateAfterPayoutAlreadyCreatedForCycle()
    {
        var sp = _services!;
        var store = sp.GetRequiredService<IDocumentStore>();
        var cycleId = Guid.NewGuid();
        var anchor = DateTime.UtcNow;
        await using (var s = store.LightweightSession())
        {
            s.Store(new SpinStateDocument
            {
                Id = EbsIds.SpinStateDocumentId,
                CycleStartUtc = anchor.AddMinutes(-1),
                SpinCycleId = cycleId,
                VerificationDeadlineUtc = anchor.AddHours(1),
                CandidateTwitchUserId = "u1",
                CandidateCharacterName = "Abcd",
                PayoutCreatedForCycle = true,
            });
            await s.SaveChangesAsync();
        }

        var m = sp.GetRequiredService<IMediator>();
        var r = await m.Send(new VerifyCandidateCommand(
            new VerifyCandidateRequest(1, cycleId, "Abcd", true, DateTime.UtcNow)));
        Assert.True(r.Ok);
    }
}
