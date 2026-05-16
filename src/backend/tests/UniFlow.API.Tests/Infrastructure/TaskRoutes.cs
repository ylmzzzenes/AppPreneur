namespace UniFlow.API.Tests.Infrastructure;

/// <summary>
/// Routes for <see cref="UniFlow.API.Controllers.TaskController"/> (<c>api/v1/task</c>).
/// </summary>
internal static class TaskRoutes
{
    private const string Base = "/api/v1/task";

    public const string List = Base;

    public static string Detail(long id) => $"{Base}/{id}";

    public static string Status(long id) => $"{Base}/{id}/status";
}
