namespace MimironsGoldOMatic.Backend.Domain.System.Dtos;

public sealed record ApiErrorDto(string Code, string Message, object Details);
