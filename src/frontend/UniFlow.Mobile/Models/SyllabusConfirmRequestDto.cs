using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class SyllabusConfirmRequestDto
{
    [JsonPropertyName("scanId")]
    public Guid ScanId { get; set; }

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = string.Empty;

    [JsonPropertyName("courseTitle")]
    public string CourseTitle { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<SyllabusDetectedItemDto> Items { get; set; } = [];
}
