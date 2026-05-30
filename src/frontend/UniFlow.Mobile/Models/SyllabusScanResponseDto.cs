using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class SyllabusScanResponseDto
{
    [JsonPropertyName("scanId")]
    public Guid ScanId { get; set; }

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = string.Empty;

    [JsonPropertyName("courseTitle")]
    public string CourseTitle { get; set; } = string.Empty;

    [JsonPropertyName("detectedItems")]
    public List<SyllabusDetectedItemDto> DetectedItems { get; set; } = [];

    [JsonPropertyName("sourceSummary")]
    public string SourceSummary { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
}
