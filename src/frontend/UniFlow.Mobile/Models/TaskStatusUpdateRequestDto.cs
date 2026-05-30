using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class TaskStatusUpdateRequestDto
{
    [JsonPropertyName("status")]
    public TaskItemStatus Status { get; set; }
}
