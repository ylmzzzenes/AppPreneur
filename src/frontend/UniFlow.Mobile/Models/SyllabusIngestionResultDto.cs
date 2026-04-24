using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class SyllabusIngestionResultDto
{
    [JsonPropertyName("courseId")]
    public long CourseId { get; set; }

    [JsonPropertyName("syllabusId")]
    public long SyllabusId { get; set; }

    [JsonPropertyName("taskCount")]
    public int TaskCount { get; set; }
}
