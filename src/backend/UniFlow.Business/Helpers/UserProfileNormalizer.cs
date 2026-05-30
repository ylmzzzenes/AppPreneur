namespace UniFlow.Business.Helpers;

internal static class UserProfileNormalizer
{
    public static string? NormalizeDisplayName(string? displayName) =>
        string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();

    public static string? NormalizeMajor(string? major) =>
        string.IsNullOrWhiteSpace(major) ? null : major.Trim();

    public static string? NormalizeAcademicGoal(string? academicGoal) =>
        string.IsNullOrWhiteSpace(academicGoal) ? null : academicGoal.Trim();
}
