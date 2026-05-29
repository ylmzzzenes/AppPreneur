using UniFlow.Entity.Common;

namespace UniFlow.Entity.Entities;

public sealed class Syllabus : BaseEntity
{
    public long CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of source text (lowercase hex). Full text is not stored by default.
    /// </summary>
    public string? SourceTextHash { get; set; }

    /// <summary>
    /// Truncated preview of extracted source text for display/audit only.
    /// </summary>
    public string? SourceTextPreview { get; set; }

    /// <summary>
    /// Optional truncated raw source text when raw storage is enabled in application configuration.
    /// </summary>
    public string? SourceText { get; set; }
    public int? SourceTextLength { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
