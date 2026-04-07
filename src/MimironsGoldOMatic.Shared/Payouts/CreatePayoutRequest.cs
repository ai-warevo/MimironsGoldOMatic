namespace MimironsGoldOMatic.Shared.Payouts;

public sealed record CreatePayoutRequest(
    string CharacterName,
    string EnrollmentRequestId);
