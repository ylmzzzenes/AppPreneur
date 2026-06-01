using System.Text.Json.Serialization;
using UniFlow.Mobile.Models;

namespace UniFlow.Mobile.Models;

public sealed class TaskFeedbackRequestDto
{
    [JsonPropertyName("taskId")]
    public long TaskId { get; init; }

    [JsonPropertyName("newStatus")]
    public TaskItemStatus NewStatus { get; init; }
}

public sealed class TaskFeedbackResponseDto
{
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("tone")]
    public string Tone { get; init; } = string.Empty;

    [JsonPropertyName("nextAction")]
    public string NextAction { get; init; } = string.Empty;

    [JsonPropertyName("isFallback")]
    public bool IsFallback { get; init; }
}
