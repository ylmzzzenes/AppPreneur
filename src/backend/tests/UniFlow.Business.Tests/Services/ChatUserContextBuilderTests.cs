using FluentAssertions;
using NSubstitute;
using UniFlow.Business.Services;
using UniFlow.DataAccess.Queries;
using UniFlow.Entity.Enums;
using UniFlow.Entity.ReadModels;
using Xunit;

namespace UniFlow.Business.Tests.Services;

public sealed class ChatUserContextBuilderTests
{
    [Fact]
    public async Task BuildAsync_IncludesCoursesAndTasks()
    {
        var userQueries = Substitute.For<IUserQueries>();
        userQueries.GetAiProfileContextAsync(7, Arg.Any<CancellationToken>())
            .Returns(new AiUserProfileContext { DisplayName = "Ayşe", Major = "Bilgisayar" });

        var courseQueries = Substitute.For<ICourseQueries>();
        courseQueries.ListForUserAsync(7, Arg.Any<CancellationToken>())
            .Returns(new List<CourseSummary>
            {
                new() { Code = "BM443", Title = "Veri Yapıları", ActiveTaskCount = 1, TaskCount = 3 },
            });

        var taskQueries = Substitute.For<ITaskQueries>();
        taskQueries.ListForUserAsync(7, Arg.Any<CancellationToken>())
            .Returns(new List<TaskItemSummary>
            {
                new()
                {
                    Title = "Proje teslimi",
                    CourseCode = "BM443",
                    Status = TaskItemStatus.Pending,
                    DueDate = new DateTime(2026, 6, 1),
                    PriorityScore = 90,
                },
            });

        var text = await ChatUserContextBuilder.BuildAsync(7, userQueries, courseQueries, taskQueries, CancellationToken.None);

        text.Should().Contain("BM443");
        text.Should().Contain("Proje teslimi");
        text.Should().Contain("Bekliyor");
        text.Should().Contain("Ayşe");
    }
}
