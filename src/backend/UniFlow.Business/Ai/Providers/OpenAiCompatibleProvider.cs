using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniFlow.Business.Configuration;

namespace UniFlow.Business.Ai.Providers;

public sealed class OpenAiCompatibleProvider
{
    public const string HttpClientName = "UniFlow.Ai.OpenAiCompatible";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly ILogger<OpenAiCompatibleProvider> _logger;

    public OpenAiCompatibleProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<AiOptions> options,
        ILogger<OpenAiCompatibleProvider> logger)
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
            AiRequestLogger.LogFailed(_logger, AiProviders.OpenAiCompatible, "AI_CONFIG");
            throw new AiProviderException("AI_CONFIG", "AI API key is not configured.", AiProviders.OpenAiCompatible);
        }

        if (string.IsNullOrWhiteSpace(request.UserPrompt) && string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            throw new AiProviderException("AI_PROMPT", "Prompt is empty.", AiProviders.OpenAiCompatible);
        }

        var model = string.IsNullOrWhiteSpace(request.Model) ? _options.Model.Trim() : request.Model.Trim();
        var baseUrl = NormalizeBaseUrl(_options.BaseUrl);
        var url = $"{baseUrl}/chat/completions";

        var messages = new List<object>();
        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(new { role = "system", content = request.SystemPrompt });
        }

        messages.Add(new { role = "user", content = request.UserPrompt });

        var body = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["messages"] = messages,
        };

        if (request.Temperature.HasValue)
        {
            body["temperature"] = request.Temperature.Value;
        }

        if (request.MaxOutputTokens.HasValue)
        {
            body["max_tokens"] = request.MaxOutputTokens.Value;
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        httpRequest.Content = JsonContent.Create(body, options: SerializerOptions);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            AiRequestLogger.LogFailed(_logger, AiProviders.OpenAiCompatible, "AI_HTTP", (int)response.StatusCode);
            throw new AiProviderException(
                "AI_HTTP",
                $"AI request failed with status {(int)response.StatusCode}.",
                AiProviders.OpenAiCompatible);
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrEmpty(text))
            {
                AiRequestLogger.LogFailed(_logger, AiProviders.OpenAiCompatible, "AI_EMPTY");
                throw new AiProviderException("AI_EMPTY", "AI returned an empty response.", AiProviders.OpenAiCompatible);
            }

            var inputEstimate = AiTokenEstimator.Estimate(request);
            return new AiTextResponse
            {
                Content = text,
                Provider = AiProviders.OpenAiCompatible,
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
            AiRequestLogger.LogFailed(_logger, AiProviders.OpenAiCompatible, "AI_PARSE");
            throw new AiProviderException("AI_PARSE", "Could not parse AI response.", AiProviders.OpenAiCompatible);
        }
    }

    public static string NormalizeBaseUrl(string baseUrl)
    {
        var trimmed = baseUrl.Trim().TrimEnd('/');
        return trimmed;
    }
}
