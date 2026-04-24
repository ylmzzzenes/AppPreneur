using Microsoft.Extensions.Options;
using Tesseract;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Configuration;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services.Ocr;

/// <summary>
/// Local OCR for images (PNG, JPEG, TIFF). PDFs should use Azure in most setups.
/// </summary>
public sealed class TesseractOcrService : IOcrService
{
    private readonly UniFlowOcrOptions _options;

    public TesseractOcrService(IOptions<UniFlowOcrOptions> options)
    {
        _options = options.Value;
    }

    public Task<Result<string>> ExtractTextAsync(byte[] content, string? contentType, CancellationToken cancellationToken = default)
    {
        if (content.Length == 0)
        {
            return Task.FromResult(Result<string>.Fail("OCR_EMPTY", "Document content is empty."));
        }

        var tess = _options.Tesseract;
        if (string.IsNullOrWhiteSpace(tess.DataPath))
        {
            return Task.FromResult(Result<string>.Fail(
                "OCR_TESSERACT_CONFIG",
                "Tesseract DataPath is not configured (folder with tessdata)."));
        }

        try
        {
            using var engine = new TesseractEngine(tess.DataPath, tess.Language, EngineMode.Default);
            using var image = Pix.LoadFromMemory(content);
            using var page = engine.Process(image);
            var text = page.GetText();
            return Task.FromResult(Result<string>.Success(text));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<string>.Fail("OCR_TESSERACT_FAILED", ex.Message));
        }
    }
}
