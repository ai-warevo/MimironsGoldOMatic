namespace MimironsGoldOMatic.Shared.Payouts;

/// <summary>JSON body for <c>POST /api/payouts/{id}/confirm-acceptance</c>.</summary>
public sealed record ConfirmAcceptanceRequest(string CharacterName);
