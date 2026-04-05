// <!-- Updated: 2026-04-05 (Tier B integration & first run) -->
using System.Text.Json.Serialization;
using MimironsGoldOMatic.Shared;

namespace MimironsGoldOMatic.Backend.Api;

public sealed record VerifyCandidateRequest(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("spinCycleId")] Guid SpinCycleId,
    [property: JsonPropertyName("characterName")] string CharacterName,
    [property: JsonPropertyName("online")] bool Online,
    [property: JsonPropertyName("capturedAt")] DateTime CapturedAt);

public sealed record RouletteStateResponse(
    [property: JsonPropertyName("nextSpinAt")] DateTime NextSpinAt,
    [property: JsonPropertyName("serverNow")] DateTime ServerNow,
    [property: JsonPropertyName("spinIntervalSeconds")] int SpinIntervalSeconds,
    [property: JsonPropertyName("poolParticipantCount")] int PoolParticipantCount,
    [property: JsonPropertyName("spinPhase")] string SpinPhase,
    [property: JsonPropertyName("currentSpinCycleId")] Guid? CurrentSpinCycleId);

public sealed record PoolMeResponse(
    [property: JsonPropertyName("isEnrolled")] bool IsEnrolled,
    [property: JsonPropertyName("characterName")] string? CharacterName);

public sealed record PatchPayoutStatusRequest(
    [property: JsonPropertyName("status")] PayoutStatus Status);

public sealed record ConfirmAcceptanceRequest(
    [property: JsonPropertyName("characterName")] string CharacterName);

public sealed record PoolEnrollmentResponse(
    [property: JsonPropertyName("characterName")] string CharacterName,
    [property: JsonPropertyName("enrollmentRequestId")] string EnrollmentRequestId);

/// <summary>CI Tier B only — POST <c>/api/e2e/prepare-pending-payout</c> (Development + <c>Mgm:EnableE2eHarness</c>).</summary>
public sealed record E2ePreparePendingRequest(
    [property: JsonPropertyName("twitchUserId")] string TwitchUserId);

/// <summary>CI Tier B only — payout created after harness + verify-candidate.</summary>
public sealed record E2ePreparePendingResponse(
    [property: JsonPropertyName("payoutId")] Guid PayoutId,
    [property: JsonPropertyName("characterName")] string CharacterName);
