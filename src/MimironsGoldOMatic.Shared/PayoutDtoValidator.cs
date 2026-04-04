using FluentValidation;

namespace MimironsGoldOMatic.Shared;

public sealed class PayoutDtoValidator : AbstractValidator<PayoutDto>
{
    public PayoutDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TwitchUserId).NotEmpty();
        RuleFor(x => x.TwitchDisplayName).NotEmpty();
        RuleFor(x => x.CharacterName)
            .NotEmpty()
            .Must(CharacterNameRules.IsValid)
            .WithMessage(
                "CharacterName must be 2–12 Unicode letters in Latin or Cyrillic scripts only (docs/SPEC.md §4).");
        RuleFor(x => x.GoldAmount)
            .Equal(PayoutEconomics.MvpWinningPayoutGold)
            .WithMessage($"GoldAmount must be {PayoutEconomics.MvpWinningPayoutGold}g for MVP winning payouts (docs/SPEC.md §2).");
        RuleFor(x => x.EnrollmentRequestId).NotEmpty();
    }
}
