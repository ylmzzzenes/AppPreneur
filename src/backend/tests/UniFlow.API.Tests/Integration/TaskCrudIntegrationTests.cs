using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UniFlow.API.Tests.Infrastructure;
using UniFlow.Business.Contracts.Courses;
using UniFlow.Business.Contracts.Tasks;
using UniFlow.Entity.Enums;
using Xunit;

namespace UniFlow.API.Tests.Integration;

public sealed class TaskCrudIntegrationTests : IClassFixture<UniFlowApiFactory>
{
    private readonly UniFlowApiFactory _factory;
    private readonly HttpClient _client;

    public TaskCrudIntegrationTests(UniFlowApiFactory factory)
    {
        _factory = factory;
        factory.ResetDatabaseAsync().GetAwaiter().GetResult();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTask_UnderOwnCourse_ReturnsSuccess()
    {
        var (token, courseId) = await CreateCourseAsync("task-create");
        using var bearer = _factory.CreateBearerClient(token);

        var request = new CreateTaskRequest
        {
            CourseId = courseId,
            Title = "Read chapter 1",
            Description = "Focus on graphs",
            DueDate = DateTime.UtcNow.Date,
            EstimatedMinutes = 45,
            PriorityScore = 70,
        };

        var response = await bearer.PostAsJsonAsync(TaskRoutes.List, request);
        var body = await AuthTestHelper.ReadResultAsync<TaskItemResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.IsSuccess.Should().BeTrue();
        body.Data!.Title.Should().Be("Read chapter 1");
        body.Data.IsAiGenerated.Should().BeFalse();
    }

    [Fact]
    public async Task CreateTask_OtherUsersCourse_ReturnsNotFound()
    {
        var (tokenA, courseId) = await CreateCourseAsync("task-other-course-a");
        var tokenB = await RegisterTokenAsync("task-other-course-b");

        using var clientB = _factory.CreateBearerClient(tokenB);
        var response = await clientB.PostAsJsonAsync(
            TaskRoutes.List,
            new CreateTaskRequest { CourseId = courseId, Title = "Hack" });
        var body = await AuthTestHelper.ReadResultAsync<TaskItemResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body!.Error!.Code.Should().Be("COURSE_NOT_FOUND");
    }

    [Fact]
    public async Task UpdateTask_Owner_ReturnsSuccess()
    {
        var (token, courseId) = await CreateCourseAsync("task-update");
        using var bearer = _factory.CreateBearerClient(token);

        var create = await bearer.PostAsJsonAsync(
            TaskRoutes.List,
            new CreateTaskRequest { CourseId = courseId, Title = "Old title" });
        var created = (await AuthTestHelper.ReadResultAsync<TaskItemResponse>(create))!.Data!;

        var update = await bearer.PutAsJsonAsync(
            TaskRoutes.Detail(created.Id),
            new UpdateTaskRequest
            {
                CourseId = courseId,
                Title = "New title",
                Status = TaskItemStatus.Pending,
            });
        var body = await AuthTestHelper.ReadResultAsync<TaskItemResponse>(update);

        update.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Data!.Title.Should().Be("New title");
    }

    [Fact]
    public async Task UpdateTask_OtherUser_ReturnsNotFound()
    {
        var seed = await TaskTestSeedHelper.CreateTwoUsersWithTaskForUserAAsync(_client, _factory);
        using var clientB = _factory.CreateBearerClient(seed.UserB.AccessToken);

        var response = await clientB.PutAsJsonAsync(
            TaskRoutes.Detail(seed.UserATaskId),
            new UpdateTaskRequest
            {
                CourseId = 1,
                Title = "Stolen",
                Status = TaskItemStatus.Done,
            });
        var body = await AuthTestHelper.ReadResultAsync<TaskItemResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body!.Error!.Code.Should().Be("TASK_NOT_FOUND");
    }

    [Fact]
    public async Task DeleteTask_Owner_ReturnsSuccess()
    {
        var (token, courseId) = await CreateCourseAsync("task-delete");
        using var bearer = _factory.CreateBearerClient(token);

        var create = await bearer.PostAsJsonAsync(
            TaskRoutes.List,
            new CreateTaskRequest { CourseId = courseId, Title = "Temp" });
        var created = (await AuthTestHelper.ReadResultAsync<TaskItemResponse>(create))!.Data!;

        var delete = await bearer.DeleteAsync(TaskRoutes.Detail(created.Id));
        var body = await AuthTestHelper.ReadResultAsync<bool>(delete);

        delete.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTask_OtherUser_ReturnsNotFound()
    {
        var seed = await TaskTestSeedHelper.CreateTwoUsersWithTaskForUserAAsync(_client, _factory);
        using var clientB = _factory.CreateBearerClient(seed.UserB.AccessToken);

        var response = await clientB.DeleteAsync(TaskRoutes.Detail(seed.UserATaskId));
        var body = await AuthTestHelper.ReadResultAsync<bool>(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body!.Error!.Code.Should().Be("TASK_NOT_FOUND");
    }

    [Fact]
    public async Task GetTodayTasks_ReturnsTodayDueTasks()
    {
        var (token, courseId) = await CreateCourseAsync("task-today");
        using var bearer = _factory.CreateBearerClient(token);

        await bearer.PostAsJsonAsync(
            TaskRoutes.List,
            new CreateTaskRequest
            {
                CourseId = courseId,
                Title = "Today task",
                DueDate = DateTime.UtcNow.Date,
            });

        await bearer.PostAsJsonAsync(
            TaskRoutes.List,
            new CreateTaskRequest
            {
                CourseId = courseId,
                Title = "Future task",
                DueDate = DateTime.UtcNow.Date.AddDays(5),
            });

        var response = await bearer.GetAsync($"{TaskRoutes.List}/today");
        var body = await AuthTestHelper.ReadResultAsync<TaskListResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Data!.Items.Should().ContainSingle(t => t.Title == "Today task");
    }

    [Fact]
    public async Task GetUpcomingTasks_ReturnsFutureTasks()
    {
        var (token, courseId) = await CreateCourseAsync("task-upcoming");
        using var bearer = _factory.CreateBearerClient(token);

        await bearer.PostAsJsonAsync(
            TaskRoutes.List,
            new CreateTaskRequest
            {
                CourseId = courseId,
                Title = "Soon",
                DueDate = DateTime.UtcNow.Date.AddDays(2),
            });

        var response = await bearer.GetAsync($"{TaskRoutes.List}/upcoming?days=7");
        var body = await AuthTestHelper.ReadResultAsync<IReadOnlyList<TaskItemResponse>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Data.Should().ContainSingle(t => t.Title == "Soon");
    }

    [Fact]
    public async Task PatchStatus_StillWorks()
    {
        var seed = await TaskTestSeedHelper.CreateTwoUsersWithTaskForUserAAsync(_client, _factory);
        using var clientA = _factory.CreateBearerClient(seed.UserA.AccessToken);

        var response = await TaskTestHelper.PatchStatusAsync(clientA, seed.UserATaskId, TaskItemStatus.Done);
        var body = await AuthTestHelper.ReadResultAsync<TaskItemResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Data!.Status.Should().Be(TaskItemStatus.Done);
    }

    private async Task<(string Token, long CourseId)> CreateCourseAsync(string suffix)
    {
        var token = await RegisterTokenAsync(suffix);
        using var bearer = _factory.CreateBearerClient(token);
        var response = await bearer.PostAsJsonAsync(
            CourseRoutes.List,
            new CreateCourseRequest { Code = $"C{suffix[..8]}", Title = "Test Course" });
        var body = await AuthTestHelper.ReadResultAsync<CourseResponse>(response);
        body!.IsSuccess.Should().BeTrue();
        return (token, body.Data!.Id);
    }

    private async Task<string> RegisterTokenAsync(string suffix)
    {
        var auth = await AuthTestHelper.RegisterAsync(_client, AuthTestHelper.CreateValidRegisterRequest(suffix));
        auth.IsSuccess.Should().BeTrue();
        return auth.Data!.AccessToken;
    }
}
