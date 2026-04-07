namespace MimironsGoldOMatic.Backend.Application.Roulette.Commands;

public sealed record VerifyCandidateCommand(VerifyCandidateRequest Body) : IRequest<HandlerResult<Unit>>;
