using Marten;
using Marten.Events;
using JasperFx.Events;

namespace MimironsGoldOMatic.Backend.Persistence;

/// <summary>Shared Marten document/event registration for EBS and integration tests.</summary>
public static class MgmMartenDocumentConfiguration
{
    public static void Configure(StoreOptions opts)
    {
        opts.DatabaseSchemaName = "mgm";
        opts.RegisterDocumentType<PoolDocument>();
        opts.RegisterDocumentType<SpinStateDocument>();
        opts.RegisterDocumentType<PayoutReadDocument>();
        opts.RegisterDocumentType<GiftRequestReadDocument>();
        opts.RegisterDocumentType<GiftCommandUsageDocument>();
        opts.RegisterDocumentType<ChatMessageDedupDocument>();
        opts.RegisterDocumentType<EnrollmentIdempotencyDocument>();
        opts.Events.StreamIdentity = StreamIdentity.AsGuid;
        opts.Events.AddEventType(typeof(PayoutCreated));
        opts.Events.AddEventType(typeof(PayoutStatusChanged));
        opts.Events.AddEventType(typeof(WinnerAcceptanceRecorded));
        opts.Events.AddEventType(typeof(HelixRewardSentAnnouncementSucceeded));
        opts.Events.AddEventType(typeof(GiftRequestInitiated));
        opts.Events.AddEventType(typeof(GiftRequestStateChanged));
        opts.Events.AddEventType(typeof(GiftItemSelected));
    }
}

