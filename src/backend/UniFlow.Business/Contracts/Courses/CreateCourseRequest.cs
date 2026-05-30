namespace UniFlow.Business.Contracts.Courses;

public sealed class CreateCourseRequest
{
    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Color { get; set; }
}
