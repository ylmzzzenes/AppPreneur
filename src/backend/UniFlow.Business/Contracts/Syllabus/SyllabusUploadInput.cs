namespace UniFlow.Business.Contracts.Syllabus;

/// <summary>
/// Upload payload without ASP.NET types so validation and ingestion stay in the business layer.
/// </summary>
public sealed class SyllabusUploadInput
{
    public required Stream Content { get; init; }

    public string FileName { get; init; } = string.Empty;

    public string? ContentType { get; init; }

    /// <summary>
    /// Declared size from the client (e.g. IFormFile.Length). Used for early rejection before reading the stream.
    /// </summary>
    public long? DeclaredLength { get; init; }
}
