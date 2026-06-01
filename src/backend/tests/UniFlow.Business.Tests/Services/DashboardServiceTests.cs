using FluentAssertions;
using NSubstitute;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Dashboard;
using UniFlow.Business.Services;
using UniFlow.DataAccess.Queries;
using UniFlow.Entity.Enums;
using UniFlow.Entity.ReadModels;
using Xunit;

namespace UniFlow.Business.Tests.Services;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task GetTodayAsync_UsesPersonalityVibeInDailyMessage()
    {
        var dashboardQueries = Substitute.For<IDashboardQueries>();
        dashboardQueries
            .ListTaskRowsForUserAsync(1, Arg.Any<CancellationToken>())
            .Returns([]);

        var userQueries = Substitute.For<IUserQueries>();
        userQueries
            .GetPersonalityVibeAsync(1, Arg.Any<CancellationToken>())
            .Returns(PersonalityVibe.Motivational);
        userQueries
            .GetAiProfileContextAsync(1, Arg.Any<CancellationToken>())
            .Returns(new AiUserProfileContext { PersonalityVibe = PersonalityVibe.Motivational });

        var dailyMessage = Substitute.For<IPersonalizedDailyMessageService>();
        dailyMessage
            .BuildDailyMessageAsync(Arg.Any<DailyMessageContext>(), Arg.Any<AiUserProfileContext?>(), Arg.Any<CancellationToken>())
            .Returns("Test daily message");

        var sut = new DashboardService(dashboardQueries, userQueries, dailyMessage);

        var result = await sut.GetTodayAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PersonalityVibe.Should().Be(PersonalityVibe.Motivational);
        result.Data.DailyMessage.Should().Be("Test daily message");
        result.Data.AiMood.Should().NotBeNullOrWhiteSpace();
    }
}
