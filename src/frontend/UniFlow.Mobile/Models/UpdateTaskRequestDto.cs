using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class UpdateTaskRequestDto
{
    public long CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public int? EstimatedMinutes { get; set; }

    public int? PriorityScore { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskItemStatus Status { get; set; }
}
