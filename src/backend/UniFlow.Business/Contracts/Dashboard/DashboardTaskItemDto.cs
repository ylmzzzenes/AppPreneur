using UniFlow.Entity.Enums;

namespace UniFlow.Business.Contracts.Dashboard;

public sealed class DashboardTaskItemDto
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public string? Category { get; set; }

    public int? PriorityScore { get; set; }

    public TaskItemStatus Status { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseTitle { get; set; } = string.Empty;

    public bool IsOverdue { get; set; }
}
