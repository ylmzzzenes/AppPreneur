using FluentAssertions;
using NSubstitute;
using UniFlow.Business.Contracts.Dashboard;
using UniFlow.Business.Services;
using UniFlow.DataAccess.Queries;
using UniFlow.Entity.Enums;
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

        var sut = new DashboardService(dashboardQueries, userQueries, new DailyMessageService());

        var result = await sut.GetTodayAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PersonalityVibe.Should().Be(PersonalityVibe.Motivational);
        result.Data.DailyMessage.Should().NotBeNullOrWhiteSpace();
        result.Data.AiMood.Should().NotBeNullOrWhiteSpace();
    }
}
