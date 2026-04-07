namespace MimironsGoldOMatic.Shared.Payouts;

/// <summary>JSON body for <c>POST /api/roulette/verify-candidate</c> (Desktop tail of <c>WoWChatLog.txt</c> and Backend).</summary>
public sealed record VerifyCandidateRequest(
    int SchemaVersion,
    Guid SpinCycleId,
    string CharacterName,
    bool Online,
    DateTime CapturedAt);
