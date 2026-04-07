namespace MimironsGoldOMatic.Backend.Application.Roulette.Commands;

public sealed record ConfirmAcceptanceCommand(Guid Id, string CharacterName) : IRequest<HandlerResult<Unit>>;
