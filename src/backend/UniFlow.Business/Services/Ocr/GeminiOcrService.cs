using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Ai;
using UniFlow.Business.Ai.Providers;
using UniFlow.Business.Configuration;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services.Ocr;

/// <summary>
/// Uses Gemini multimodal generateContent to extract plain text from syllabus images/PDFs.
/// Reuses Ai:ApiKey and Ai:Model — no separate OCR key required in Development.
/// </summary>
public sealed class GeminiOcrService(
    IHttpClientFactory httpClientFactory,
    IOptions<AiOptions> aiOptions,
    ILogger<GeminiOcrService> logger) : IOcrService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private const string ExtractionPrompt =
        "Bu görsel bir üniversite ders müfredatı veya ders bilgi sayfasıdır. " +
        "Görünen tüm metni düz metin olarak çıkar. Türkçe karakterleri koru. " +
        "Tablo yapısını satır satır okunabilir şekilde aktar. " +
        "Yalnızca çıkarılan metni döndür; açıklama veya markdown ekleme.";

    public async Task<Result<string>> ExtractTextAsync(
        byte[] content,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        if (content.Length == 0)
        {
            return Result<string>.Fail("OCR_EMPTY", "Document content is empty.");
        }

        var ai = aiOptions.Value;
        if (string.IsNullOrWhiteSpace(ai.ApiKey))
        {
            return Result<string>.Fail(
                "OCR_GEMINI_CONFIG",
                "Gemini OCR için Ai:ApiKey gerekli. user-secrets ile AIzaSy... key ekleyin veya OCR provider olarak Tesseract/Azure kullanın.");
        }

        var mimeType = ResolveMimeType(contentType);
        if (mimeType is null)
        {
            return Result<string>.Fail(
                "OCR_GEMINI_UNSUPPORTED",
                "Desteklenmeyen dosya türü. PNG, JPEG veya PDF yükleyin.");
        }

        var model = ai.Model.Trim();
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent";

        var body = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = ExtractionPrompt },
                        new
                        {
                            inlineData = new
                            {
                                mimeType,
                                data = Convert.ToBase64String(content),
                            },
                        },
                    },
                },
            },
        };

        try
        {
            var httpClient = httpClientFactory.CreateClient(GeminiAiProvider.HttpClientName);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.TryAddWithoutValidation("x-goog-api-key", ai.ApiKey);
            httpRequest.Content = JsonContent.Create(body, options: SerializerOptions);

            using var response = await httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var status = (int)response.StatusCode;
                logger.LogWarning("Gemini OCR HTTP {StatusCode}", status);
                return Result<string>.Fail(
                    "OCR_GEMINI_HTTP",
                    status is 401 or 403
                        ? "Gemini API key geçersiz. Google AI Studio'dan AIzaSy... key kullanın."
                        : $"Gemini OCR isteği başarısız (HTTP {status}).");
            }

            var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(raw);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
            {
                return Result<string>.Fail("OCR_GEMINI_EMPTY", "Gemini görselden metin çıkaramadı.");
            }

            logger.LogInformation(
                "Gemini OCR completed. MimeType={MimeType}, InputBytes={InputBytes}, OutputLength={OutputLength}",
                mimeType,
                content.Length,
                text.Length);

            return Result<string>.Success(text.Trim());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gemini OCR failed.");
            return Result<string>.Fail("OCR_GEMINI_FAILED", "Gemini OCR başarısız oldu.");
        }
    }

    private static string? ResolveMimeType(string? contentType) =>
        contentType?.Trim().ToLowerInvariant() switch
        {
            "image/png" => "image/png",
            "image/jpeg" => "image/jpeg",
            "application/pdf" => "application/pdf",
            _ => null,
        };
}
