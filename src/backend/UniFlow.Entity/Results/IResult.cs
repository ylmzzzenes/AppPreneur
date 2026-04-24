namespace UniFlow.Entity.Results;

public interface IResult
{
    bool IsSuccess { get; }

    Error? Error { get; }
}
