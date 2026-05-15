namespace UniFlow.Business.Contracts.Syllabus;

public sealed class SyllabusConfirmRequest
{
    public Guid ScanId { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseTitle { get; set; } = string.Empty;

    public IReadOnlyList<SyllabusDetectedItemDto> Items { get; set; } = Array.Empty<SyllabusDetectedItemDto>();
}
