using MimironsGoldOMatic.Desktop.Api;
using MimironsGoldOMatic.Shared;

namespace MimironsGoldOMatic.Desktop.Services;

/// <summary>Backend EBS calls used by the Desktop app (mock in unit tests).</summary>
public interface IEbsDesktopClient
{
    Task<IReadOnlyList<PayoutDto>> GetPendingAsync(CancellationToken ct);

    Task PatchPayoutStatusAsync(Guid id, PayoutStatus status, CancellationToken ct);

    Task ConfirmAcceptanceAsync(Guid id, string characterName, CancellationToken ct);

    Task VerifyCandidateAsync(VerifyCandidateRequestDto dto, CancellationToken ct);
}
