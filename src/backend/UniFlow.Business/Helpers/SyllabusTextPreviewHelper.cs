namespace UniFlow.Business.Helpers;

internal static class SyllabusTextPreviewHelper
{
    public static string BuildPreview(string? sourceText, int maxLength)
    {
        if (string.IsNullOrEmpty(sourceText))
        {
            return string.Empty;
        }

        return sourceText.Length <= maxLength ? sourceText : sourceText[..maxLength];
    }
}
