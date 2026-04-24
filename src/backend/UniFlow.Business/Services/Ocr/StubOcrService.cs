using System.Text;
using UniFlow.Business.Abstractions;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services.Ocr;

/// <summary>
/// Development fallback: returns UTF-8 text if the payload looks like plain text; otherwise fails.
/// </summary>
public sealed class StubOcrService : IOcrService
{
    public Task<Result<string>> ExtractTextAsync(byte[] content, string? contentType, CancellationToken cancellationToken = default)
    {
        if (content.Length == 0)
        {
            return Task.FromResult(Result<string>.Fail("OCR_EMPTY", "Document content is empty."));
        }

        if (IsMostlyUtf8Text(content))
        {
            var text = Encoding.UTF8.GetString(content);
            return Task.FromResult(Result<string>.Success(text));
        }

        return Task.FromResult(Result<string>.Fail(
            "OCR_STUB_BINARY",
            "Stub OCR cannot read binary documents. Configure Azure, Tesseract, or AWS provider."));
    }

    private static bool IsMostlyUtf8Text(byte[] data)
    {
        var sample = Math.Min(data.Length, 4096);
        var printable = 0;
        for (var i = 0; i < sample; i++)
        {
            var b = data[i];
            if (b is >= 32 and <= 126 or 9 or 10 or 13)
            {
                printable++;
            }
            else if (b >= 128)
            {
                printable++;
            }
        }

        return sample > 0 && printable * 100 / sample >= 85;
    }
}
