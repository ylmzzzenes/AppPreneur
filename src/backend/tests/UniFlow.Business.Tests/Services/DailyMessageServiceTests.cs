using FluentAssertions;
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

        friendly.Should().NotBe(strict);
        strict.Should().ContainEquivalentOf("gecik");
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

        message.Should().Contain("5");
        message.Should().Match(m =>
            m.Contains("tamam", StringComparison.OrdinalIgnoreCase)
            || m.Contains("iş", StringComparison.OrdinalIgnoreCase)
            || m.Contains("Muhteşem", StringComparison.OrdinalIgnoreCase));
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

        _sut.BuildDailyMessage(context).Should().Be(_sut.BuildDailyMessage(context));
    }

    [Fact]
    public void BuildDailyMessage_PersonalityVibe_ChangesMessageForSameStats()
    {
        var bigThree = new[] { new DashboardTaskItemDto { Title = "Reading" } };

        var calm = _sut.BuildDailyMessage(new DailyMessageContext
        {
            UserId = 12,
            Today = Today,
            PersonalityVibe = PersonalityVibe.Calm,
            BigThreeTasks = bigThree,
        });

        var sarcastic = _sut.BuildDailyMessage(new DailyMessageContext
        {
            UserId = 12,
            Today = Today,
            PersonalityVibe = PersonalityVibe.Sarcastic,
            BigThreeTasks = bigThree,
        });

        calm.Should().NotBe(sarcastic);
    }
}
