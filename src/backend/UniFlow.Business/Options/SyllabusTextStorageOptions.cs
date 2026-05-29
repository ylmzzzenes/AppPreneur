namespace UniFlow.Business.Options;

/// <summary>
/// Controls how OCR/AI syllabus source text and scan previews are persisted.
/// </summary>
public sealed class SyllabusTextStorageOptions
{
    public const string SectionName = "SyllabusTextStorage";

    public int MaxStoredSourceTextLength { get; set; } = 4000;

    public int MaxStoredPreviewJsonLength { get; set; } = 20_000;

    public bool StoreRawSourceText { get; set; }

    public bool StoreAiRawResponse { get; set; }

    public bool StorePreviewJson { get; set; } = true;

    public bool NormalizeBeforeHashing { get; set; } = true;
}
