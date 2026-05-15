namespace UniFlow.Business.Contracts.Syllabus;

public sealed class ValidatedSyllabusFile
{
    public required byte[] Content { get; init; }

    public required string ContentType { get; init; }

    public required string FileName { get; init; }
}
