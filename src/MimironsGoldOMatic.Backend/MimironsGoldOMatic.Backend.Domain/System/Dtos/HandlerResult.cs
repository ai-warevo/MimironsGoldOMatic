namespace MimironsGoldOMatic.Backend.Domain.System.Dtos;

public sealed record HandlerResult<T>(bool Ok, T? Value, int StatusCode, ApiErrorDto? Error);
