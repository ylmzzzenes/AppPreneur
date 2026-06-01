using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class StudyPlanRequestDto
{
    [JsonPropertyName("courseId")]
    public long? CourseId { get; init; }

    [JsonPropertyName("days")]
    public int Days { get; init; } = 7;

    [JsonPropertyName("focus")]
    public string? Focus { get; init; }
}

public sealed class StudyPlanResponseDto
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;

    [JsonPropertyName("days")]
    public List<StudyPlanDayResponseDto> Days { get; init; } = [];

    [JsonPropertyName("isFallback")]
    public bool IsFallback { get; init; }
}

public sealed class StudyPlanDayResponseDto
{
    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("focus")]
    public string Focus { get; init; } = string.Empty;

    [JsonPropertyName("tasks")]
    public List<StudyPlanTaskResponseDto> Tasks { get; init; } = [];

    [JsonPropertyName("tip")]
    public string Tip { get; init; } = string.Empty;
}

public sealed class StudyPlanTaskResponseDto
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("estimatedMinutes")]
    public int EstimatedMinutes { get; init; }

    [JsonPropertyName("reason")]
    public string Reason { get; init; } = string.Empty;
}
