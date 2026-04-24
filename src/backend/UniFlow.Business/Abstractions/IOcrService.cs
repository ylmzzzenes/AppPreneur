using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

/// <summary>
/// Extracts plain text from syllabus images or documents (PDF/image bytes).
/// </summary>
public interface IOcrService
{
    Task<Result<string>> ExtractTextAsync(
        byte[] content,
        string? contentType,
        CancellationToken cancellationToken = default);
}
