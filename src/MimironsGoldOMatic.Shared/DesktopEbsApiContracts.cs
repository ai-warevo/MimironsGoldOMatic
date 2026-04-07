namespace MimironsGoldOMatic.Shared;

/// <summary>JSON body for <c>POST /api/roulette/verify-candidate</c> (Desktop tail of <c>WoWChatLog.txt</c> and Backend).</summary>
public sealed record VerifyCandidateRequest(
    int SchemaVersion,
    Guid SpinCycleId,
    string CharacterName,
    bool Online,
    DateTime CapturedAt);

/// <summary>JSON body for <c>PATCH /api/payouts/{id}/status</c>.</summary>
public sealed record PatchPayoutStatusRequest(PayoutStatus Status);

/// <summary>JSON body for <c>POST /api/payouts/{id}/confirm-acceptance</c>.</summary>
public sealed record ConfirmAcceptanceRequest(string CharacterName);
