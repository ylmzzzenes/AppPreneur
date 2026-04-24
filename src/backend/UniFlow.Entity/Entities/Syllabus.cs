using UniFlow.Entity.Common;

namespace UniFlow.Entity.Entities;

public sealed class Syllabus : BaseEntity
{
    public long CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Raw text from OCR or uploaded document extraction.
    /// </summary>
    public string? SourceText { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
