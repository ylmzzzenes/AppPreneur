using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UniFlow.API.Tests.Infrastructure;
using UniFlow.Business.Contracts.Courses;
using UniFlow.Business.Contracts.Tasks;
using UniFlow.Entity.Enums;
using Xunit;

namespace UniFlow.API.Tests.Integration;

public sealed class CourseCrudIntegrationTests : IClassFixture<UniFlowApiFactory>
{
    private readonly UniFlowApiFactory _factory;
    private readonly HttpClient _client;

    public CourseCrudIntegrationTests(UniFlowApiFactory factory)
    {
        _factory = factory;
        factory.ResetDatabaseAsync().GetAwaiter().GetResult();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCourse_AuthenticatedUser_ReturnsSuccess()
    {
        var auth = await RegisterAndGetTokenAsync("course-create");
        using var bearer = _factory.CreateBearerClient(auth);

        var request = new CreateCourseRequest
        {
            Code = "CS101",
            Title = "Intro to CS",
            Description = "Basics",
            Color = "#6366F1",
        };

        var response = await bearer.PostAsJsonAsync(CourseRoutes.List, request);
        var body = await AuthTestHelper.ReadResultAsync<CourseResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.IsSuccess.Should().BeTrue();
        body.Data!.Code.Should().Be("CS101");
        body.Data.TaskCount.Should().Be(0);
    }

    [Fact]
    public async Task ListCourses_ReturnsOwnCourses()
    {
        var auth = await RegisterAndGetTokenAsync("course-list");
        using var bearer = _factory.CreateBearerClient(auth);

        await bearer.PostAsJsonAsync(CourseRoutes.List, new CreateCourseRequest { Code = "MAT", Title = "Math" });

        var response = await bearer.GetAsync(CourseRoutes.List);
        var body = await AuthTestHelper.ReadResultAsync<IReadOnlyList<CourseResponse>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCourse_OtherUser_ReturnsNotFound()
    {
        var authA = await RegisterAndGetTokenAsync("course-owner-a");
        var authB = await RegisterAndGetTokenAsync("course-owner-b");
        using var clientA = _factory.CreateBearerClient(authA);

        var create = await clientA.PostAsJsonAsync(
            CourseRoutes.List,
            new CreateCourseRequest { Code = "PHY", Title = "Physics" });
        var created = await AuthTestHelper.ReadResultAsync<CourseResponse>(create);

        using var clientB = _factory.CreateBearerClient(authB);
        var response = await clientB.GetAsync(CourseRoutes.Detail(created!.Data!.Id));
        var body = await AuthTestHelper.ReadResultAsync<CourseResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body!.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCourse_DuplicateCode_ReturnsConflict()
    {
        var auth = await RegisterAndGetTokenAsync("course-dup");
        using var bearer = _factory.CreateBearerClient(auth);

        var request = new CreateCourseRequest { Code = "CS101", Title = "First" };
        await bearer.PostAsJsonAsync(CourseRoutes.List, request);

        var duplicate = await bearer.PostAsJsonAsync(
            CourseRoutes.List,
            new CreateCourseRequest { Code = "cs101", Title = "Second" });
        var body = await AuthTestHelper.ReadResultAsync<CourseResponse>(duplicate);

        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
        body!.Error!.Code.Should().Be("COURSE_CODE_DUPLICATE");
    }

    [Fact]
    public async Task DeleteCourse_WithTasks_ReturnsBadRequest()
    {
        var auth = await RegisterAndGetTokenAsync("course-delete-tasks");
        using var bearer = _factory.CreateBearerClient(auth);

        var courseResponse = await bearer.PostAsJsonAsync(
            CourseRoutes.List,
            new CreateCourseRequest { Code = "DEL101", Title = "Delete Me" });
        var course = (await AuthTestHelper.ReadResultAsync<CourseResponse>(courseResponse))!.Data!;

        await bearer.PostAsJsonAsync(
            TaskRoutes.List,
            new CreateTaskRequest
            {
                CourseId = course.Id,
                Title = "Homework 1",
                DueDate = DateTime.UtcNow.Date.AddDays(2),
            });

        var deleteResponse = await bearer.DeleteAsync(CourseRoutes.Detail(course.Id));
        var deleteBody = await AuthTestHelper.ReadResultAsync<bool>(deleteResponse);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        deleteBody!.Error!.Code.Should().Be("COURSE_HAS_TASKS");
    }

    private async Task<string> RegisterAndGetTokenAsync(string suffix)
    {
        var auth = await AuthTestHelper.RegisterAsync(_client, AuthTestHelper.CreateValidRegisterRequest(suffix));
        auth.IsSuccess.Should().BeTrue();
        return auth.Data!.AccessToken;
    }
}
