namespace UniFlow.Entity.Results;

/// <summary>
/// Machine-oriented code plus human-readable message for failures.
/// </summary>
public sealed record Error(string Code, string Message);
