using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class TaskItemResponseDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("dueDate")]
    public DateTime? DueDate { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("priorityScore")]
    public int? PriorityScore { get; set; }

    [JsonPropertyName("courseId")]
    public long CourseId { get; set; }

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = string.Empty;

    [JsonPropertyName("courseTitle")]
    public string CourseTitle { get; set; } = string.Empty;

    [JsonPropertyName("syllabusId")]
    public long SyllabusId { get; set; }

    [JsonPropertyName("syllabusTitle")]
    public string SyllabusTitle { get; set; } = string.Empty;
}
