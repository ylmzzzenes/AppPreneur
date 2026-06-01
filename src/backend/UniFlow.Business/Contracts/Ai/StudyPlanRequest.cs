namespace UniFlow.Business.Contracts.Ai;

public sealed class StudyPlanRequest
{
    public long? CourseId { get; init; }

    public int Days { get; init; } = 7;

    public string? Focus { get; init; }
}
