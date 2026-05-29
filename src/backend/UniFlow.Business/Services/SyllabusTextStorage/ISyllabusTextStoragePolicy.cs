namespace UniFlow.Business.Services.SyllabusTextStorage;

/// <summary>
/// Hashing, truncation, and persistence rules for syllabus OCR/AI source text.
/// </summary>
public interface ISyllabusTextStoragePolicy
{
    string ComputeSha256Hash(string? input);

    string? PrepareSourceTextForStorage(string? sourceText);

    string? PreparePreviewJsonForStorage(string? previewJson);

    string? PrepareAiRawResponseForStorage(string? aiRawResponse);

    int GetTextLength(string? input);
}
