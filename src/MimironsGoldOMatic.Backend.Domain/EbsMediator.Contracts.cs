using MediatR;
using MimironsGoldOMatic.Backend.Abstract;

namespace MimironsGoldOMatic.Backend.Domain;

public sealed record ApiErrorDto(string Code, string Message, object Details);

public sealed record VerifyCandidateRequest(
    int SchemaVersion,
    Guid SpinCycleId,
    string CharacterName,
    bool Online,
    DateTime CapturedAt);

public sealed record RouletteStateResponse(
    DateTime NextSpinAt,
    DateTime ServerNow,
    int SpinIntervalSeconds,
    int PoolParticipantCount,
    string SpinPhase,
    Guid? CurrentSpinCycleId);

public sealed record PoolMeResponse(bool IsEnrolled, string? CharacterName);

public sealed record PoolEnrollmentResponse(string CharacterName, string EnrollmentRequestId);

public sealed record PatchPayoutStatusRequest(PayoutStatus Status);

public sealed record ConfirmAcceptanceRequest(string CharacterName);

public sealed record HandlerResult<T>(bool Ok, T? Value, int StatusCode, ApiErrorDto? Error);

public sealed record PostClaimCommand(string TwitchUserId, string TwitchDisplayName, CreatePayoutRequest Body)
    : IRequest<HandlerResult<PoolEnrollmentResponse>>;

public sealed record GetRouletteStateQuery : IRequest<HandlerResult<RouletteStateResponse>>;

public sealed record GetPoolMeQuery(string TwitchUserId) : IRequest<HandlerResult<PoolMeResponse>>;

public sealed record GetPendingPayoutsQuery : IRequest<HandlerResult<IReadOnlyList<PayoutDto>>>;

public sealed record GetMyLastPayoutQuery(string TwitchUserId) : IRequest<HandlerResult<PayoutDto?>>;

public sealed record PatchPayoutStatusCommand(Guid Id, PayoutStatus NewStatus) : IRequest<HandlerResult<PayoutDto>>;

public sealed record ConfirmAcceptanceCommand(Guid Id, string CharacterName) : IRequest<HandlerResult<Unit>>;

public sealed record VerifyCandidateCommand(VerifyCandidateRequest Body) : IRequest<HandlerResult<Unit>>;

public sealed record GiftSelectedItemDto(
    string Name,
    int Id,
    int Count,
    string Link,
    string Texture,
    int BagId,
    int SlotId);

public enum GiftRequestStateDto
{
    Pending,
    SelectingItem,
    ItemSelected,
    WaitingConfirmation,
    Completed,
    Failed,
}

public sealed record GiftRequestDto(
    Guid Id,
    string StreamerId,
    string ViewerId,
    string ViewerDisplayName,
    string CharacterName,
    GiftRequestStateDto State,
    GiftSelectedItemDto? SelectedItem,
    int QueuePosition,
    int EstimatedWaitSeconds,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? TimeoutAt,
    string? FailureReason);

public sealed record CreateGiftRequest(string StreamerId, string CharacterName);

public sealed record GetGiftQueueQuery(string? StreamerId, string? ViewerId) : IRequest<HandlerResult<IReadOnlyList<GiftRequestDto>>>;

public sealed record CreateGiftRequestCommand(string ViewerId, string ViewerDisplayName, CreateGiftRequest Body)
    : IRequest<HandlerResult<GiftRequestDto>>;

public sealed record PatchGiftRequestCommand(Guid Id, GiftRequestStateDto State, string? Reason)
    : IRequest<HandlerResult<GiftRequestDto>>;

public sealed record SelectGiftItemCommand(Guid Id, GiftSelectedItemDto Item) : IRequest<HandlerResult<GiftRequestDto>>;

public sealed record ConfirmGiftCommand(Guid Id, bool Confirmed) : IRequest<HandlerResult<GiftRequestDto>>;

public sealed record PatchGiftRequestState(GiftRequestStateDto State, string? Reason);

public sealed record SelectGiftItemRequest(GiftSelectedItemDto Item);

public sealed record ConfirmGiftRequest(bool Confirmed);

/// <summary>CI Tier B only — POST <c>/api/e2e/prepare-pending-payout</c> (Development + <c>Mgm:EnableE2eHarness</c>).</summary>
public sealed record E2ePreparePendingRequest(string TwitchUserId);

/// <summary>CI Tier B only — payout created after harness + verify-candidate.</summary>
public sealed record E2ePreparePendingResponse(Guid PayoutId, string CharacterName);
