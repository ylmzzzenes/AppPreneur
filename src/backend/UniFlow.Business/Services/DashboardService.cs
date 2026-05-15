using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Dashboard;
using UniFlow.DataAccess.Queries;
using UniFlow.Entity.Enums;
using UniFlow.Entity.ReadModels;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services;

public sealed class DashboardService(
    IDashboardQueries dashboardQueries,
    IUserQueries userQueries,
    IDailyMessageService dailyMessageService) : IDashboardService
{
    public async Task<Result<DashboardTodayResponse>> GetTodayAsync(long userId, CancellationToken cancellationToken = default)
    {
        var personalityVibe = await userQueries.GetPersonalityVibeAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? PersonalityVibe.Friendly;

        var today = DateTime.UtcNow.Date;
        var rows = await dashboardQueries.ListTaskRowsForUserAsync(userId, cancellationToken).ConfigureAwait(false);

        var incomplete = rows.Where(r => r.Status != TaskItemStatus.Done).ToList();
        var overdue = incomplete.Where(r => IsOverdue(r, today)).ToList();

        var bigThree = incomplete
            .OrderByDescending(r => IsOverdue(r, today))
            .ThenByDescending(r => r.PriorityScore ?? 0)
            .ThenBy(r => r.DueDate ?? DateTime.MaxValue)
            .Take(3)
            .Select(r => MapTaskDto(r, today))
            .ToList();

        var completedToday = rows.Count(r =>
            r.Status == TaskItemStatus.Done &&
            r.UpdatedDate.HasValue &&
            r.UpdatedDate.Value.Date == today);

        var pendingToday = rows.Count(r =>
            r.Status == TaskItemStatus.Pending &&
            r.DueDate.HasValue &&
            r.DueDate.Value.Date == today);

        var overdueCount = overdue.Count;
        var aiMood = ResolveAiMood(overdueCount, completedToday);

        var dailyMessage = dailyMessageService.BuildDailyMessage(new DailyMessageContext
        {
            UserId = userId,
            Today = today,
            PersonalityVibe = personalityVibe,
            OverdueTasksCount = overdueCount,
            CompletedTodayCount = completedToday,
            PendingTodayCount = pendingToday,
            BigThreeTasks = bigThree,
        });

        return Result<DashboardTodayResponse>.Success(new DashboardTodayResponse
        {
            Today = today,
            BigThreeTasks = bigThree,
            OverdueTasksCount = overdueCount,
            CompletedTodayCount = completedToday,
            PendingTodayCount = pendingToday,
            PersonalityVibe = personalityVibe,
            AiMood = aiMood,
            DailyMessage = dailyMessage,
        });
    }

    private static bool IsOverdue(DashboardTaskRow row, DateTime today) =>
        row.DueDate.HasValue && row.DueDate.Value.Date < today;

    private static DashboardTaskItemDto MapTaskDto(DashboardTaskRow row, DateTime today) => new()
    {
        Id = row.Id,
        Title = row.Title,
        DueDate = row.DueDate,
        Category = row.Category,
        PriorityScore = row.PriorityScore,
        Status = row.Status,
        CourseCode = row.CourseCode,
        CourseTitle = row.CourseTitle,
        IsOverdue = IsOverdue(row, today),
    };

    private static string ResolveAiMood(int overdueCount, int completedTodayCount)
    {
        if (overdueCount > 3)
        {
            return "Sarcastic";
        }

        if (overdueCount > 0)
        {
            return "Angry";
        }

        if (completedTodayCount >= 3)
        {
            return "Happy";
        }

        return "Neutral";
    }
}
