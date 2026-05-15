using UniFlow.Business.Contracts.Dashboard;
using UniFlow.Business.Services;
using UniFlow.Entity.Enums;
using Xunit;

namespace UniFlow.Business.Tests.Services;

public sealed class DailyMessageServiceTests
{
    private readonly DailyMessageService _sut = new();

    private static readonly DateTime Today = new(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void BuildDailyMessage_StrictWithOverdue_DiffersFromFriendly()
    {
        var bigThree = new[] { new DashboardTaskItemDto { Title = "Midterm Exam" } };

        var friendly = _sut.BuildDailyMessage(new DailyMessageContext
        {
            UserId = 42,
            Today = Today,
            PersonalityVibe = PersonalityVibe.Friendly,
            OverdueTasksCount = 2,
            BigThreeTasks = bigThree,
        });

        var strict = _sut.BuildDailyMessage(new DailyMessageContext
        {
            UserId = 42,
            Today = Today,
            PersonalityVibe = PersonalityVibe.Strict,
            OverdueTasksCount = 2,
            BigThreeTasks = bigThree,
        });

        Assert.NotEqual(friendly, strict);
        Assert.Contains("gecik", strict, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildDailyMessage_HighCompletedCount_ReturnsPositiveProductiveMessage()
    {
        var message = _sut.BuildDailyMessage(new DailyMessageContext
        {
            UserId = 7,
            Today = Today,
            PersonalityVibe = PersonalityVibe.Motivational,
            OverdueTasksCount = 0,
            CompletedTodayCount = 5,
            PendingTodayCount = 0,
            BigThreeTasks = [],
        });

        Assert.Contains("5", message, StringComparison.Ordinal);
        Assert.True(
            message.Contains("tamam", StringComparison.OrdinalIgnoreCase)
            || message.Contains("iş", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Muhteşem", StringComparison.OrdinalIgnoreCase),
            $"Expected productive tone, got: {message}");
    }

    [Fact]
    public void BuildDailyMessage_SameUserSameDay_IsDeterministic()
    {
        var context = new DailyMessageContext
        {
            UserId = 99,
            Today = Today,
            PersonalityVibe = PersonalityVibe.Calm,
            OverdueTasksCount = 0,
            CompletedTodayCount = 1,
            PendingTodayCount = 2,
            BigThreeTasks = [new DashboardTaskItemDto { Title = "Lab Report" }],
        };

        var first = _sut.BuildDailyMessage(context);
        var second = _sut.BuildDailyMessage(context);

        Assert.Equal(first, second);
    }

    [Fact]
    public void BuildDailyMessage_PersonalityVibe_ChangesMessageForSameStats()
    {
        var context = new DailyMessageContext
        {
            UserId = 12,
            Today = Today,
            OverdueTasksCount = 0,
            CompletedTodayCount = 0,
            PendingTodayCount = 0,
            BigThreeTasks = [new DashboardTaskItemDto { Title = "Reading" }],
        };

        var calm = _sut.BuildDailyMessage(new DailyMessageContext
        {
            UserId = context.UserId,
            Today = context.Today,
            PersonalityVibe = PersonalityVibe.Calm,
            OverdueTasksCount = context.OverdueTasksCount,
            BigThreeTasks = context.BigThreeTasks,
        });

        var sarcastic = _sut.BuildDailyMessage(new DailyMessageContext
        {
            UserId = context.UserId,
            Today = context.Today,
            PersonalityVibe = PersonalityVibe.Sarcastic,
            OverdueTasksCount = context.OverdueTasksCount,
            BigThreeTasks = context.BigThreeTasks,
        });

        Assert.NotEqual(calm, sarcastic);
    }
}
