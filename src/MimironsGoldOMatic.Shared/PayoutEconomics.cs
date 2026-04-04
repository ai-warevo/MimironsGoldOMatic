namespace MimironsGoldOMatic.Shared;

/// <summary>
/// MVP payout economics per <c>docs/SPEC.md</c> §2 (fixed winning payout amount).
/// </summary>
public static class PayoutEconomics
{
    /// <summary>Gold units per winning payout in MVP (not copper).</summary>
    public const long MvpWinningPayoutGold = 1000;
}
