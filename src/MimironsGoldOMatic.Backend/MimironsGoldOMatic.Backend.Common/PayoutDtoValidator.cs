using FluentValidation;

namespace MimironsGoldOMatic.Backend.Common;

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
                "CharacterName must be 2вЂ“12 Unicode letters in Latin or Cyrillic scripts only (docs/overview/SPEC.md В§4).");
        RuleFor(x => x.GoldAmount)
            .Equal(PayoutEconomics.MvpWinningPayoutGold)
            .WithMessage($"GoldAmount must be {PayoutEconomics.MvpWinningPayoutGold}g for MVP winning payouts (docs/overview/SPEC.md В§2).");
        RuleFor(x => x.EnrollmentRequestId).NotEmpty();
    }
}
