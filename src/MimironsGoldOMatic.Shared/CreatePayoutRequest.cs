namespace MimironsGoldOMatic.Shared;

public sealed record CreatePayoutRequest(
    string CharacterName,
    string TwitchTransactionId);
