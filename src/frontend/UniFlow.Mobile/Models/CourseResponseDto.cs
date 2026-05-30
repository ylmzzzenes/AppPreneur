namespace UniFlow.Mobile.Models;

public sealed class CourseResponseDto
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Color { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int TaskCount { get; set; }

    public int ActiveTaskCount { get; set; }
}
