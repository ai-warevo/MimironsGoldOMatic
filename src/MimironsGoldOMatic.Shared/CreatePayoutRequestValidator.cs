using FluentValidation;

namespace MimironsGoldOMatic.Shared;

public sealed class CreatePayoutRequestValidator : AbstractValidator<CreatePayoutRequest>
{
    public CreatePayoutRequestValidator()
    {
        RuleFor(x => x.CharacterName)
            .NotEmpty()
            .Length(2, 12)
            .Must(s => s.All(char.IsLetter))
            .WithMessage("CharacterName must be 2–12 letters (see docs/SPEC.md §4 for Latin/Cyrillic script rules).");

        RuleFor(x => x.EnrollmentRequestId)
            .NotEmpty();
    }
}
