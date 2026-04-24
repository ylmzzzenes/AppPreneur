using UniFlow.Business.Abstractions;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services.Ocr;

/// <summary>
/// Placeholder for Amazon Textract integration (configure SDK + IAM in a later iteration).
/// </summary>
public sealed class AwsTextractOcrService : IOcrService
{
    public Task<Result<string>> ExtractTextAsync(byte[] content, string? contentType, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<string>.Fail(
            "OCR_AWS_NOT_CONFIGURED",
            "AWS Textract OCR is not implemented yet. Use Azure or Tesseract."));
}
