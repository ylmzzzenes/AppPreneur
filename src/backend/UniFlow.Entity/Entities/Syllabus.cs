using UniFlow.Entity.Common;

namespace UniFlow.Entity.Entities;

public sealed class Syllabus : BaseEntity
{
    public long CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of normalized OCR/AI source text (full text is not stored).
    /// </summary>
    public string? SourceTextHash { get; set; }

    /// <summary>
    /// Truncated preview of extracted source text for display/audit only.
    /// </summary>
    public string? SourceTextPreview { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
