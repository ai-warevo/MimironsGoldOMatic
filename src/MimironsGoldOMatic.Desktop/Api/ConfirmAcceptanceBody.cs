using System.Text.Json.Serialization;

namespace MimironsGoldOMatic.Desktop.Api;

public sealed record ConfirmAcceptanceBody(
    [property: JsonPropertyName("characterName")] string CharacterName);
