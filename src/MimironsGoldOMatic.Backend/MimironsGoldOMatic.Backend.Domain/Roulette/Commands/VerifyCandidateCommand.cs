namespace MimironsGoldOMatic.Backend.Domain.Roulette.Commands;

public sealed record VerifyCandidateCommand(VerifyCandidateRequest Body) : IRequest<HandlerResult<Unit>>;
