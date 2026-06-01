using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class WeeklySummaryResponseDto
{
    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;

    [JsonPropertyName("completedCount")]
    public int CompletedCount { get; init; }

    [JsonPropertyName("missedCount")]
    public int MissedCount { get; init; }

    [JsonPropertyName("pendingCount")]
    public int PendingCount { get; init; }

    [JsonPropertyName("strongPoint")]
    public string StrongPoint { get; init; } = string.Empty;

    [JsonPropertyName("improvementPoint")]
    public string ImprovementPoint { get; init; } = string.Empty;

    [JsonPropertyName("nextWeekFocus")]
    public string NextWeekFocus { get; init; } = string.Empty;

    [JsonPropertyName("isFallback")]
    public bool IsFallback { get; init; }
}
