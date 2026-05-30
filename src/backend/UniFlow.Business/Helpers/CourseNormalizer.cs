namespace UniFlow.Business.Helpers;

internal static class CourseNormalizer
{
    public static string NormalizeCode(string code) => code.Trim();

    public static string NormalizeTitle(string title) => title.Trim();

    public static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
