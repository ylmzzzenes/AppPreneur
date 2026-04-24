namespace UniFlow.Entity.Results;

/// <summary>
/// Operation outcome without a payload.
/// </summary>
public sealed class Result : IResult
{
    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public Error? Error { get; }

    public static Result Ok() => new(true, null);

    public static Result Fail(Error error) => new(false, error);

    public static Result Fail(string code, string message) => new(false, new Error(code, message));
}

/// <summary>
/// Operation outcome with a payload of type <typeparamref name="T"/>.
/// </summary>
public sealed class Result<T> : IResult
{
    private Result(bool isSuccess, T? data, Error? error)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
    }

    public bool IsSuccess { get; }

    public T? Data { get; }

    public Error? Error { get; }

    public static Result<T> Success(T data) => new(true, data, null);

    public static Result<T> Fail(Error error) => new(false, default, error);

    public static Result<T> Fail(string code, string message) => new(false, default, new Error(code, message));
}
