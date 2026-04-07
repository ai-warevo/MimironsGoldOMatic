using MimironsGoldOMatic.Shared;

namespace MimironsGoldOMatic.Desktop.Services;

/// <summary>Backend EBS calls used by the Desktop app (mock in unit tests).</summary>
public interface IEbsDesktopClient
{
    Task<IReadOnlyList<PayoutDto>> GetPendingAsync(CancellationToken ct);

    Task PatchPayoutStatusAsync(Guid id, PayoutStatus status, CancellationToken ct);

    Task ConfirmAcceptanceAsync(Guid id, string characterName, CancellationToken ct);

    Task VerifyCandidateAsync(VerifyCandidateRequest dto, CancellationToken ct);

    Task<IReadOnlyList<GiftRequestDto>> GetGiftQueueAsync(CancellationToken ct);

    Task PatchGiftRequestStateAsync(Guid id, GiftRequestState state, string? reason, CancellationToken ct);

    Task SelectGiftItemAsync(Guid id, GiftSelectedItemDto item, CancellationToken ct);

    Task ConfirmGiftAsync(Guid id, bool confirmed, CancellationToken ct);

    Task<VersionInfoDto> GetVersionInfoAsync(CancellationToken ct);
}
