namespace UniFlow.Business.Contracts.Syllabus;

public sealed class SyllabusDetectedItemDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Task category/type (e.g. Exam, Homework).
    /// </summary>
    public string? Type { get; set; }

    public int? PriorityScore { get; set; }
}
