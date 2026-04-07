namespace MimironsGoldOMatic.Backend.Application.System.Dtos;

public sealed record ApiErrorDto(string Code, string Message, object Details);
