namespace UniFlow.Business.Contracts.Syllabus;

public sealed class SyllabusScanResponse
{
    public Guid ScanId { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseTitle { get; set; } = string.Empty;

    public IReadOnlyList<SyllabusDetectedItemDto> DetectedItems { get; set; } = Array.Empty<SyllabusDetectedItemDto>();

    public string SourceSummary { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}
