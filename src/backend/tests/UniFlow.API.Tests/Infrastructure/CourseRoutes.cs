namespace UniFlow.API.Tests.Infrastructure;

internal static class CourseRoutes
{
    public const string List = "api/v1/courses";

    public static string Detail(long id) => $"api/v1/courses/{id}";
}
