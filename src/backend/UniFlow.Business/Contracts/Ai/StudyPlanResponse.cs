namespace UniFlow.Business.Contracts.Ai;

public sealed class StudyPlanResponse
{
    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public IReadOnlyList<StudyPlanDayResponse> Days { get; set; } = Array.Empty<StudyPlanDayResponse>();

    public bool IsFallback { get; set; }
}

public sealed class StudyPlanDayResponse
{
    public string Date { get; set; } = string.Empty;

    public string Focus { get; set; } = string.Empty;

    public IReadOnlyList<StudyPlanTaskResponse> Tasks { get; set; } = Array.Empty<StudyPlanTaskResponse>();

    public string Tip { get; set; } = string.Empty;
}

public sealed class StudyPlanTaskResponse
{
    public string Title { get; set; } = string.Empty;

    public int EstimatedMinutes { get; set; }

    public string Reason { get; set; } = string.Empty;
}
