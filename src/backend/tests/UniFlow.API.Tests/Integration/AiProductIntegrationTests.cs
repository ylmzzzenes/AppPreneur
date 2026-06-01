using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UniFlow.API.Tests.Infrastructure;
using UniFlow.Business.Contracts.Ai;
using UniFlow.Business.Contracts.Courses;
using UniFlow.Business.Contracts.Tasks;
using UniFlow.Entity.Enums;
using Xunit;

namespace UniFlow.API.Tests.Integration;

public sealed class AiProductIntegrationTests : IClassFixture<UniFlowApiFactory>
{
    private readonly UniFlowApiFactory _factory;
    private readonly HttpClient _client;

    public AiProductIntegrationTests(UniFlowApiFactory factory)
    {
        _factory = factory;
        factory.ResetDatabaseAsync().GetAwaiter().GetResult();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GenerateStudyPlan_Authenticated_ReturnsSuccess()
    {
        var token = await RegisterTokenAsync("ai-plan-user");
        using var bearer = _factory.CreateBearerClient(token);

        var response = await bearer.PostAsJsonAsync("/api/v1/ai/study-plan", new StudyPlanRequest { Days = 7 });
        var body = await AuthTestHelper.ReadResultAsync<StudyPlanResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.IsSuccess.Should().BeTrue();
        body.Data!.Days.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateStudyPlan_InvalidDays_ReturnsBadRequest()
    {
        var token = await RegisterTokenAsync("ai-plan-invalid");
        using var bearer = _factory.CreateBearerClient(token);

        var response = await bearer.PostAsJsonAsync("/api/v1/ai/study-plan", new StudyPlanRequest { Days = 30 });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateStudyPlan_OtherUsersCourse_ReturnsNotFound()
    {
        var (tokenA, courseId) = await CreateCourseAsync("ai-plan-owner");
        var tokenB = await RegisterTokenAsync("ai-plan-other");

        using var clientB = _factory.CreateBearerClient(tokenB);
        var response = await clientB.PostAsJsonAsync(
            "/api/v1/ai/study-plan",
            new StudyPlanRequest { Days = 7, CourseId = courseId });
        var body = await AuthTestHelper.ReadResultAsync<StudyPlanResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body!.Error!.Code.Should().Be("COURSE_NOT_FOUND");
    }

    [Fact]
    public async Task TaskFeedback_OwnTask_ReturnsSuccess()
    {
        var (token, courseId) = await CreateCourseAsync("ai-feedback");
        using var bearer = _factory.CreateBearerClient(token);

        var create = await bearer.PostAsJsonAsync(
            TaskRoutes.List,
            new CreateTaskRequest { CourseId = courseId, Title = "Read chapter 2" });
        var task = (await AuthTestHelper.ReadResultAsync<TaskItemResponse>(create))!.Data!;

        var response = await bearer.PostAsJsonAsync(
            "/api/v1/ai/task-feedback",
            new TaskFeedbackRequest { TaskId = task.Id, NewStatus = TaskItemStatus.Done });
        var body = await AuthTestHelper.ReadResultAsync<TaskFeedbackResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.IsSuccess.Should().BeTrue();
        body.Data!.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task TaskFeedback_OtherUsersTask_ReturnsNotFound()
    {
        var (tokenA, courseId) = await CreateCourseAsync("ai-feedback-a");
        var tokenB = await RegisterTokenAsync("ai-feedback-b");

        using var clientA = _factory.CreateBearerClient(tokenA);
        var create = await clientA.PostAsJsonAsync(
            TaskRoutes.List,
            new CreateTaskRequest { CourseId = courseId, Title = "Secret task" });
        var task = (await AuthTestHelper.ReadResultAsync<TaskItemResponse>(create))!.Data!;

        using var clientB = _factory.CreateBearerClient(tokenB);
        var response = await clientB.PostAsJsonAsync(
            "/api/v1/ai/task-feedback",
            new TaskFeedbackRequest { TaskId = task.Id, NewStatus = TaskItemStatus.Done });
        var body = await AuthTestHelper.ReadResultAsync<TaskFeedbackResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body!.Error!.Code.Should().Be("TASK_NOT_FOUND");
    }

    [Fact]
    public async Task WeeklySummary_Authenticated_ReturnsSuccess()
    {
        var token = await RegisterTokenAsync("ai-weekly");
        using var bearer = _factory.CreateBearerClient(token);

        var response = await bearer.GetAsync("/api/v1/ai/weekly-summary");
        var body = await AuthTestHelper.ReadResultAsync<WeeklySummaryResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.IsSuccess.Should().BeTrue();
        body.Data!.Summary.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AiEndpoints_WithoutToken_ReturnUnauthorized()
    {
        var plan = await _client.PostAsJsonAsync("/api/v1/ai/study-plan", new StudyPlanRequest { Days = 7 });
        plan.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<string> RegisterTokenAsync(string suffix)
    {
        var auth = await AuthTestHelper.RegisterAsync(_client, AuthTestHelper.CreateValidRegisterRequest(suffix));
        auth.IsSuccess.Should().BeTrue();
        return auth.Data!.AccessToken;
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
}
