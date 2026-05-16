using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UniFlow.API.Tests.Infrastructure;
using UniFlow.Business.Contracts.Tasks;
using UniFlow.Entity.Enums;
using Xunit;

namespace UniFlow.API.Tests.Integration;

public sealed class TaskOwnershipIntegrationTests : IClassFixture<UniFlowApiFactory>
{
    private readonly UniFlowApiFactory _factory;
    private readonly HttpClient _anonymousClient;

    public TaskOwnershipIntegrationTests(UniFlowApiFactory factory)
    {
        _factory = factory;
        factory.ResetDatabaseAsync().GetAwaiter().GetResult();
        _anonymousClient = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateStatus_OwnerUser_ReturnsOk()
    {
        var seed = await TaskTestSeedHelper.CreateTwoUsersWithTaskForUserAAsync(_anonymousClient, _factory);
        using var clientA = _factory.CreateBearerClient(seed.UserA.AccessToken);

        var response = await TaskTestHelper.PatchStatusAsync(clientA, seed.UserATaskId, TaskItemStatus.Done);
        var body = await AuthTestHelper.ReadResultAsync<TaskItemResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body!.IsSuccess.Should().BeTrue();
        body.Data!.Id.Should().Be(seed.UserATaskId);
        body.Data.Status.Should().Be(TaskItemStatus.Done);
    }

    [Fact]
    public async Task UpdateStatus_OtherUser_ReturnsNotFound()
    {
        var seed = await TaskTestSeedHelper.CreateTwoUsersWithTaskForUserAAsync(_anonymousClient, _factory);
        using var clientB = _factory.CreateBearerClient(seed.UserB.AccessToken);

        var response = await TaskTestHelper.PatchStatusAsync(clientB, seed.UserATaskId, TaskItemStatus.Done);
        var body = await AuthTestHelper.ReadResultAsync<TaskItemResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body.Should().NotBeNull();
        body!.IsSuccess.Should().BeFalse();
        body.Error!.Code.Should().Be("TASK_NOT_FOUND");
    }

    [Fact]
    public async Task GetDetail_OtherUser_ReturnsNotFound()
    {
        var seed = await TaskTestSeedHelper.CreateTwoUsersWithTaskForUserAAsync(_anonymousClient, _factory);
        using var clientB = _factory.CreateBearerClient(seed.UserB.AccessToken);

        var response = await clientB.GetAsync(TaskRoutes.Detail(seed.UserATaskId));
        var body = await AuthTestHelper.ReadResultAsync<TaskItemResponse>(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        body.Should().NotBeNull();
        body!.IsSuccess.Should().BeFalse();
        body.Error!.Code.Should().Be("TASK_NOT_FOUND");
    }

    [Fact]
    public async Task List_OtherUser_DoesNotIncludeOwnersTask()
    {
        var seed = await TaskTestSeedHelper.CreateTwoUsersWithTaskForUserAAsync(_anonymousClient, _factory);
        using var clientB = _factory.CreateBearerClient(seed.UserB.AccessToken);

        var response = await clientB.GetAsync(TaskRoutes.List);
        var body = await AuthTestHelper.ReadResultAsync<IReadOnlyList<TaskItemResponse>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body!.IsSuccess.Should().BeTrue();
        body.Data.Should().NotContain(t => t.Id == seed.UserATaskId);
    }

    [Fact]
    public async Task UpdateStatus_Unauthorized_ReturnsUnauthorized()
    {
        var seed = await TaskTestSeedHelper.CreateTwoUsersWithTaskForUserAAsync(_anonymousClient, _factory);

        var response = await TaskTestHelper.PatchStatusAsync(_anonymousClient, seed.UserATaskId, TaskItemStatus.Done);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateStatus_InvalidStatus_ReturnsBadRequest()
    {
        var seed = await TaskTestSeedHelper.CreateTwoUsersWithTaskForUserAAsync(_anonymousClient, _factory);
        using var clientA = _factory.CreateBearerClient(seed.UserA.AccessToken);

        var response = await TaskTestHelper.PatchStatusRawAsync(
            clientA,
            seed.UserATaskId,
            new { status = "NotAValidStatus" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
