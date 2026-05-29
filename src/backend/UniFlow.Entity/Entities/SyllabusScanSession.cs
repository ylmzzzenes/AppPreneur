namespace UniFlow.Entity.Entities;

/// <summary>
/// Temporary storage for a syllabus scan preview before the user confirms persistence.
/// </summary>
public sealed class SyllabusScanSession
{
    public Guid Id { get; set; }

    public long UserId { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseTitle { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of extracted source text (lowercase hex). Full text is not stored.
    /// </summary>
    public string SourceTextHash { get; set; } = string.Empty;

    /// <summary>
    /// Original OCR source text length in characters (metadata only).
    /// </summary>
    public int? SourceTextLength { get; set; }

    /// <summary>
    /// Serialized preview payload (detected items and truncated summary). Size is capped by configuration.
    /// </summary>
    public string? PreviewJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public User User { get; set; } = null!;
}
