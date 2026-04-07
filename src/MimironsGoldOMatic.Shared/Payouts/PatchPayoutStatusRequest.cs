namespace MimironsGoldOMatic.Shared.Payouts;

/// <summary>JSON body for <c>PATCH /api/payouts/{id}/status</c>.</summary>
public sealed record PatchPayoutStatusRequest(PayoutStatus Status);
