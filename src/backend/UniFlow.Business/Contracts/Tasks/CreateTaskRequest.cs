using UniFlow.Entity.Enums;

namespace UniFlow.Business.Contracts.Tasks;

public sealed class CreateTaskRequest
{
    public long CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public int? EstimatedMinutes { get; set; }

    public int? PriorityScore { get; set; }

    public TaskItemStatus? Status { get; set; }
}
