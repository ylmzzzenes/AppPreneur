namespace UniFlow.Business.Contracts.Syllabus;

public sealed class SyllabusIngestionResult
{
    public long CourseId { get; set; }

    public long SyllabusId { get; set; }

    public int TaskCount { get; set; }
}
