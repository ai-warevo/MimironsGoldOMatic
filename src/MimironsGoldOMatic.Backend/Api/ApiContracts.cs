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
