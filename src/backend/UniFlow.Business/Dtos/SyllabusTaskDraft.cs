namespace UniFlow.Business.Dtos;

/// <summary>
/// Parsed syllabus row before persistence (no database identifiers).
/// </summary>
public sealed class SyllabusTaskDraft
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Category { get; set; }

    public int? PriorityScore { get; set; }
}
