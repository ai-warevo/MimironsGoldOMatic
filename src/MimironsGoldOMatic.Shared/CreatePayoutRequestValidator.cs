using FluentValidation;

namespace MimironsGoldOMatic.Shared;

public sealed class CreatePayoutRequestValidator : AbstractValidator<CreatePayoutRequest>
{
    private const int CharacterNameMaxLength = 32;

    public CreatePayoutRequestValidator()
    {
        RuleFor(x => x.CharacterName)
            .NotEmpty()
            .MaximumLength(CharacterNameMaxLength)
            .Matches(@"^[^:;]+$")
            .WithMessage("CharacterName must not contain ':' or ';'.");

        RuleFor(x => x.TwitchTransactionId)
            .NotEmpty();
    }
}
