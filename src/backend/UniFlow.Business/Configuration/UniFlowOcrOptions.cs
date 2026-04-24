namespace UniFlow.Business.Configuration;

public enum OcrProvider
{
    Stub = 0,
    Azure = 1,
    Tesseract = 2,
    Aws = 3,
}

public sealed class UniFlowOcrOptions
{
    public const string SectionName = "UniFlow:Ocr";

    public OcrProvider Provider { get; set; } = OcrProvider.Stub;

    public AzureDocumentOptions Azure { get; set; } = new();

    public TesseractOptions Tesseract { get; set; } = new();
}

public sealed class AzureDocumentOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}

public sealed class TesseractOptions
{
    /// <summary>
    /// Folder containing tessdata (e.g. eng.traineddata).
    /// </summary>
    public string DataPath { get; set; } = string.Empty;

    public string Language { get; set; } = "eng";
}
