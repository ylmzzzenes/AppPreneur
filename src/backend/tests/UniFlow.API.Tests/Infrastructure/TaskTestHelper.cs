using System.Net.Http.Json;
using UniFlow.Business.Contracts.Tasks;
using UniFlow.Entity.Enums;

namespace UniFlow.API.Tests.Infrastructure;

internal static class TaskTestHelper
{
    public static Task<HttpResponseMessage> PatchStatusAsync(
        HttpClient client,
        long taskId,
        TaskItemStatus status,
        CancellationToken cancellationToken = default) =>
        client.PatchAsJsonAsync(
            TaskRoutes.Status(taskId),
            new TaskStatusUpdateRequest { Status = status },
            cancellationToken);

    public static Task<HttpResponseMessage> PatchStatusRawAsync(
        HttpClient client,
        long taskId,
        object body,
        CancellationToken cancellationToken = default) =>
        client.PatchAsJsonAsync(TaskRoutes.Status(taskId), body, cancellationToken);
}
