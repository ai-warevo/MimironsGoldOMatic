using System.Text.Json.Serialization;

namespace MimironsGoldOMatic.Backend.Api;

public sealed record ApiErrorDto(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("details")] object Details);
