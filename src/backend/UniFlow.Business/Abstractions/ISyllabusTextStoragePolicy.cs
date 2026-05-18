using UniFlow.Business.Contracts.Syllabus;

namespace UniFlow.Business.Abstractions;

/// <summary>
/// Hashing, truncation, and persistence rules for syllabus OCR/AI source text.
/// </summary>
public interface ISyllabusTextStoragePolicy
{
    string ComputeSourceTextHash(string? sourceText);

    string BuildSourcePreview(string? sourceText);

    string? PrepareStoredSourceText(string? sourceText);

    string SerializePreview(string sourceSummary, IReadOnlyList<SyllabusDetectedItemDto> detectedItems);
}
