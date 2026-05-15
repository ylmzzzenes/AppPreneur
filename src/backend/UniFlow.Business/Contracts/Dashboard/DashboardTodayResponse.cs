using UniFlow.Entity.Enums;

namespace UniFlow.Business.Contracts.Dashboard;

public sealed class DashboardTodayResponse
{
    public DateTime Today { get; set; }

    public IReadOnlyList<DashboardTaskItemDto> BigThreeTasks { get; set; } = Array.Empty<DashboardTaskItemDto>();

    public int OverdueTasksCount { get; set; }

    public int CompletedTodayCount { get; set; }

    public int PendingTodayCount { get; set; }

    public PersonalityVibe PersonalityVibe { get; set; } = PersonalityVibe.Friendly;

    /// <summary>
    /// Persona mood derived from task stats (Sarcastic, Angry, Happy, Neutral).
    /// </summary>
    public string AiMood { get; set; } = "Neutral";

    /// <summary>
    /// Personalized daily message based on <see cref="PersonalityVibe"/> and today's stats.
    /// </summary>
    public string DailyMessage { get; set; } = string.Empty;
}
