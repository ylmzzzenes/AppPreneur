using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class DashboardTaskItemDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("dueDate")]
    public DateTime? DueDate { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("priorityScore")]
    public int? PriorityScore { get; set; }

    [JsonPropertyName("status")]
    public TaskItemStatus Status { get; set; }

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = string.Empty;

    [JsonPropertyName("courseTitle")]
    public string CourseTitle { get; set; } = string.Empty;

    [JsonPropertyName("isOverdue")]
    public bool IsOverdue { get; set; }
}
