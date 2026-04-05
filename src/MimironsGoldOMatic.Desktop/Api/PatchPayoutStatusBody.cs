using System.Text.Json.Serialization;
using MimironsGoldOMatic.Shared;

namespace MimironsGoldOMatic.Desktop.Api;

public sealed record PatchPayoutStatusBody(
    [property: JsonPropertyName("status")] PayoutStatus Status);
