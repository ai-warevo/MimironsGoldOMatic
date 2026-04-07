namespace MimironsGoldOMatic.Backend.Abstract;

public sealed record CreatePayoutRequest(
    string CharacterName,
    string EnrollmentRequestId);
