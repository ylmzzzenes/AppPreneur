using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniFlow.Business.Configuration;

namespace UniFlow.Business.Ai.Providers;

public sealed class GeminiAiProvider
{
    public const string HttpClientName = "UniFlow.Ai.Gemini";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly ILogger<GeminiAiProvider> _logger;

    public GeminiAiProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<AiOptions> options,
        ILogger<GeminiAiProvider> logger)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiTextResponse> GenerateTextAsync(
        AiTextRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            AiRequestLogger.LogFailed(_logger, AiProviders.Gemini, "AI_CONFIG");
            throw new AiProviderException("AI_CONFIG", "AI API key is not configured.", AiProviders.Gemini);
        }

        if (string.IsNullOrWhiteSpace(request.UserPrompt) && string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            throw new AiProviderException("AI_PROMPT", "Prompt is empty.", AiProviders.Gemini);
        }

        var model = string.IsNullOrWhiteSpace(request.Model) ? _options.Model.Trim() : request.Model.Trim();
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent";

        var body = BuildRequestBody(request);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Headers.TryAddWithoutValidation("x-goog-api-key", _options.ApiKey);
        httpRequest.Content = JsonContent.Create(body, options: SerializerOptions);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var status = (int)response.StatusCode;
            AiRequestLogger.LogFailed(_logger, AiProviders.Gemini, "AI_HTTP", status);
            throw new AiProviderException(
                "AI_HTTP",
                BuildHttpErrorMessage(status),
                AiProviders.Gemini);
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(text))
            {
                AiRequestLogger.LogFailed(_logger, AiProviders.Gemini, "AI_EMPTY");
                throw new AiProviderException("AI_EMPTY", "AI returned an empty response.", AiProviders.Gemini);
            }

            var inputEstimate = AiTokenEstimator.Estimate(request);
            return new AiTextResponse
            {
                Content = text,
                Provider = AiProviders.Gemini,
                Model = model,
                PromptVersion = request.PromptVersion ?? _options.PromptVersion,
                InputTokenEstimate = inputEstimate,
                OutputTokenEstimate = AiTokenEstimator.Estimate(text),
            };
        }
        catch (AiProviderException)
        {
            throw;
        }
        catch (Exception)
        {
            AiRequestLogger.LogFailed(_logger, AiProviders.Gemini, "AI_PARSE");
            throw new AiProviderException("AI_PARSE", "Could not parse AI response.", AiProviders.Gemini);
        }
    }

    private static string BuildHttpErrorMessage(int statusCode) =>
        statusCode switch
        {
            401 or 403 =>
                "Gemini API key geçersiz. Google AI Studio'dan alınan AIzaSy... ile başlayan key kullanın (user-secrets: Ai:ApiKey).",
            404 =>
                "Gemini model bulunamadı. Ai:Model değerini kontrol edin (ör. gemini-2.0-flash).",
            429 => "Gemini rate limit aşıldı. Kısa süre sonra tekrar deneyin.",
            503 =>
                "Gemini geçici olarak kullanılamıyor veya model/key hatalı. Modeli gemini-2.0-flash yapıp AIzaSy key ile deneyin.",
            _ => $"Gemini isteği başarısız (HTTP {statusCode}).",
        };

    private static object BuildRequestBody(AiTextRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            return new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = request.UserPrompt } },
                    },
                },
            };
        }

        return new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = request.SystemPrompt } },
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = request.UserPrompt } },
                },
            },
        };
    }
}
