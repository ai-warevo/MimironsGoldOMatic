namespace MimironsGoldOMatic.Backend.Domain.System.Dtos;

/// <summary>CI Tier B only - POST <c>/api/e2e/prepare-pending-payout</c> (Development + <c>Mgm:EnableE2eHarness</c>).</summary>
public sealed record E2ePreparePendingRequest(string TwitchUserId);
