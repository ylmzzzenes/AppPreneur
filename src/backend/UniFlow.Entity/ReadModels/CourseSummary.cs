namespace UniFlow.Entity.ReadModels;

public sealed class CourseSummary
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Color { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int TaskCount { get; set; }

    public int ActiveTaskCount { get; set; }
}
