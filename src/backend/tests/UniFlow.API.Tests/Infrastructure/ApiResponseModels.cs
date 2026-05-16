namespace UniFlow.API.Tests.Infrastructure;

/// <summary>
/// Mirrors API <c>Result&lt;T&gt;</c> JSON shape for integration test deserialization.
/// </summary>
internal sealed class ApiResultDto<T>
{
    public bool IsSuccess { get; set; }

    public T? Data { get; set; }

    public ApiErrorDto? Error { get; set; }
}

internal sealed class ApiErrorDto
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
