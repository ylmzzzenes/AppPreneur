using UniFlow.Entity.Common;

namespace UniFlow.Entity.Entities;

public sealed class TaskItem : BaseEntity
{
    public long SyllabusId { get; set; }

    public Syllabus Syllabus { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    /// <summary>
    /// e.g. Midterm, Final, Homework.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Adaptive scheduling score (1–100), populated by business logic when available.
    /// </summary>
    public int? PriorityScore { get; set; }
}
