namespace UniFlow.API.Contracts;

public sealed class RateLimitResponse
{
    public bool Success { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public static RateLimitResponse Exceeded() => new()
    {
        Success = false,
        Code = "RATE_LIMIT_EXCEEDED",
        Message = "Too many requests. Please try again later.",
    };
}
