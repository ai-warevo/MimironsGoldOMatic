using FluentValidation;

namespace MimironsGoldOMatic.Shared;

public sealed class PayoutDtoValidator : AbstractValidator<PayoutDto>
{
    private const int CharacterNameMaxLength = 32;

    public PayoutDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TwitchUserId).NotEmpty();
        RuleFor(x => x.TwitchDisplayName).NotEmpty();
        RuleFor(x => x.CharacterName)
            .NotEmpty()
            .MaximumLength(CharacterNameMaxLength)
            .Matches(@"^[^:;]+$")
            .WithMessage("CharacterName must not contain ':' or ';'.");
        RuleFor(x => x.GoldAmount).GreaterThan(0);
        RuleFor(x => x.TwitchTransactionId).NotEmpty();
    }
}
