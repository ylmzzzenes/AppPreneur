using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class SyllabusDetectedItemDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("dueDate")]
    public DateTime? DueDate { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("priorityScore")]
    public int? PriorityScore { get; set; }
}
