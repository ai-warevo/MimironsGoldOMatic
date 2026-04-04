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
            .Length(2, 12)
            .Must(s => s.All(char.IsLetter))
            .WithMessage("CharacterName must be 2–12 letters (see docs/SPEC.md §4 for Latin/Cyrillic script rules).");
        RuleFor(x => x.GoldAmount).GreaterThan(0);
        RuleFor(x => x.EnrollmentRequestId).NotEmpty();
    }
}
