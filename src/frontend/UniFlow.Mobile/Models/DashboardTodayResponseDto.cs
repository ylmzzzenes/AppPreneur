using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class DashboardTodayResponseDto
{
    [JsonPropertyName("today")]
    public DateTime Today { get; set; }

    [JsonPropertyName("bigThreeTasks")]
    public List<DashboardTaskItemDto> BigThreeTasks { get; set; } = [];

    [JsonPropertyName("overdueTasksCount")]
    public int OverdueTasksCount { get; set; }

    [JsonPropertyName("completedTodayCount")]
    public int CompletedTodayCount { get; set; }

    [JsonPropertyName("pendingTodayCount")]
    public int PendingTodayCount { get; set; }

    [JsonPropertyName("personalityVibe")]
    public string PersonalityVibe { get; set; } = string.Empty;

    [JsonPropertyName("aiMood")]
    public string AiMood { get; set; } = string.Empty;

    [JsonPropertyName("dailyMessage")]
    public string DailyMessage { get; set; } = string.Empty;
}
