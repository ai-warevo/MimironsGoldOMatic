using System.Text.Json.Serialization;

namespace MimironsGoldOMatic.Desktop.Api;

/// <summary>JSON body for <c>POST /api/roulette/verify-candidate</c> — mirrors Backend <c>VerifyCandidateRequest</c> (<c>docs/overview/SPEC.md</c> §8).</summary>
public sealed record VerifyCandidateRequestDto(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("spinCycleId")] Guid SpinCycleId,
    [property: JsonPropertyName("characterName")] string CharacterName,
    [property: JsonPropertyName("online")] bool Online,
    [property: JsonPropertyName("capturedAt")] DateTime CapturedAt);
