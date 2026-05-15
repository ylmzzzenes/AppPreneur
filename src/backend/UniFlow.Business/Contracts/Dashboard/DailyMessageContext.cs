using UniFlow.Entity.Enums;

namespace UniFlow.Business.Contracts.Dashboard;

/// <summary>
/// Inputs for deterministic daily message generation (no external AI calls).
/// </summary>
public sealed class DailyMessageContext
{
    public long UserId { get; init; }

    public DateTime Today { get; init; }

    public PersonalityVibe PersonalityVibe { get; init; }

    public int OverdueTasksCount { get; init; }

    public int CompletedTodayCount { get; init; }

    public int PendingTodayCount { get; init; }

    public IReadOnlyList<DashboardTaskItemDto> BigThreeTasks { get; init; } = Array.Empty<DashboardTaskItemDto>();
}
