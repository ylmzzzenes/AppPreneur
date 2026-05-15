namespace UniFlow.Business.Helpers;

internal static class UserProfileNormalizer
{
    public static string? NormalizeMajor(string? major) =>
        string.IsNullOrWhiteSpace(major) ? null : major.Trim();
}
