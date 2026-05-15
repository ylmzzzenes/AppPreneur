namespace UniFlow.Business.Contracts.Dashboard;

public sealed class DashboardTodayResponse
{
    public DateTime Today { get; set; }

    public IReadOnlyList<DashboardTaskItemDto> BigThreeTasks { get; set; } = Array.Empty<DashboardTaskItemDto>();

    public int OverdueTasksCount { get; set; }

    public int CompletedTodayCount { get; set; }

    public int PendingTodayCount { get; set; }

    public string AiMood { get; set; } = "Neutral";

    public string Message { get; set; } = string.Empty;
}
