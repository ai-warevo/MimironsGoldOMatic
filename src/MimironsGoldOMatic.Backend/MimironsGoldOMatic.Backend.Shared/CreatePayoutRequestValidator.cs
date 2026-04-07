using FluentValidation;

namespace MimironsGoldOMatic.Backend.Shared;

public sealed class CreatePayoutRequestValidator : AbstractValidator<CreatePayoutRequest>
{
    public CreatePayoutRequestValidator()
    {
        RuleFor(x => x.CharacterName)
            .NotEmpty()
            .Must(CharacterNameRules.IsValid)
            .WithMessage(
                "CharacterName must be 2вЂ“12 Unicode letters in Latin or Cyrillic scripts only (docs/overview/SPEC.md В§4).");

        RuleFor(x => x.EnrollmentRequestId)
            .NotEmpty();
    }
}
