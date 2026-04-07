namespace MimironsGoldOMatic.Backend.Domain.System.Dtos;

/// <summary>CI Tier B only - payout created after harness + verify-candidate.</summary>
public sealed record E2ePreparePendingResponse(Guid PayoutId, string CharacterName);
