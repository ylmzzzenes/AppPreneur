using UniFlow.Entity.Enums;

namespace UniFlow.Entity.ReadModels;

public sealed class TaskItemSummary
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Category { get; set; }

    public int? PriorityScore { get; set; }

    public TaskItemStatus Status { get; set; }

    public long CourseId { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseTitle { get; set; } = string.Empty;

    public long SyllabusId { get; set; }

    public string SyllabusTitle { get; set; } = string.Empty;
}
