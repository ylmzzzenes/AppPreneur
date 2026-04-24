using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Configuration;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services.Gemini;

public sealed class GeminiService : IGeminiService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly UniFlowGeminiOptions _options;

    public GeminiService(HttpClient httpClient, IOptions<UniFlowGeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<Result<string>> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return Result<string>.Fail("GEMINI_CONFIG", "Gemini API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(prompt))
        {
            return Result<string>.Fail("GEMINI_PROMPT", "Prompt is empty.");
        }

        var model = string.IsNullOrWhiteSpace(_options.Model) ? "gemini-2.0-flash" : _options.Model.Trim();
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent?key={Uri.EscapeDataString(_options.ApiKey)}";

        var body = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } },
                },
            },
        };

        using var response = await _httpClient.PostAsJsonAsync(url, body, SerializerOptions, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Result<string>.Fail("GEMINI_HTTP", $"Gemini request failed ({(int)response.StatusCode}): {raw}");
        }

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return string.IsNullOrEmpty(text)
                ? Result<string>.Fail("GEMINI_EMPTY", "Gemini returned an empty response.")
                : Result<string>.Success(text);
        }
        catch (Exception ex)
        {
            return Result<string>.Fail("GEMINI_PARSE", $"Could not parse Gemini response: {ex.Message}. Body: {raw}");
        }
    }
}
