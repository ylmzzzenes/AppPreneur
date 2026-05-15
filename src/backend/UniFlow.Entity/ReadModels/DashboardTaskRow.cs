using UniFlow.Entity.Enums;

namespace UniFlow.Entity.ReadModels;

/// <summary>
/// Lightweight projection for dashboard task queries.
/// </summary>
public sealed class DashboardTaskRow
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public string? Category { get; set; }

    public int? PriorityScore { get; set; }

    public TaskItemStatus Status { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseTitle { get; set; } = string.Empty;

    public DateTime? UpdatedDate { get; set; }
}
