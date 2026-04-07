namespace MimironsGoldOMatic.Backend.Domain.Roulette.Commands;

public sealed record ConfirmAcceptanceCommand(Guid Id, string CharacterName) : IRequest<HandlerResult<Unit>>;
