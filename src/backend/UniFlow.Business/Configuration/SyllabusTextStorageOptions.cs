namespace UniFlow.Business.Configuration;

/// <summary>
/// Controls how OCR/AI syllabus source text and scan previews are persisted.
/// </summary>
public sealed class SyllabusTextStorageOptions
{
    public const string SectionName = "SyllabusTextStorage";

    /// <summary>
    /// Maximum characters stored in <see cref="Entity.Entities.Syllabus.SourceTextPreview"/>
    /// and in scan preview summaries.
    /// </summary>
    public int MaxStoredSourceTextLength { get; set; } = 4000;

    /// <summary>
    /// Maximum characters stored in <see cref="Entity.Entities.SyllabusScanSession.PreviewJson"/>.
    /// </summary>
    public int MaxStoredPreviewJsonLength { get; set; } = 20_000;

    /// <summary>
    /// When false, full/truncated OCR source text is not written to the syllabus row
    /// (only hash and preview are stored).
    /// </summary>
    public bool StoreRawSourceText { get; set; }
}
