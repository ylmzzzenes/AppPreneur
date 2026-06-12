using Microsoft.Extensions.Hosting;
using UniFlow.Business.Ai;

namespace UniFlow.Business.Configuration;

public sealed record AiConfigurationSnapshot
{
    public required string Environment { get; init; }

    public required string Provider { get; init; }

    public required string Model { get; init; }

    public string? BaseUrl { get; init; }

    public required bool ApiKeyConfigured { get; init; }

    public required bool EnableFallback { get; init; }

    public required string EffectiveBehavior { get; init; }

    public required string ChatSystemPromptSource { get; init; }
}

public static class AiConfigurationDiagnostics
{
    public static AiConfigurationSnapshot Create(string environmentName, AiOptions options)
    {
        var provider = options.Provider.Trim();
        var apiKeyConfigured = !string.IsNullOrWhiteSpace(options.ApiKey);
        var isDevelopment = string.Equals(environmentName, Environments.Development, StringComparison.OrdinalIgnoreCase);
        var isTesting = string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase);

        return new AiConfigurationSnapshot
        {
            Environment = environmentName,
            Provider = provider,
            Model = options.Model.Trim(),
            BaseUrl = string.Equals(provider, AiProviders.OpenAiCompatible, StringComparison.OrdinalIgnoreCase)
                ? options.BaseUrl.Trim()
                : null,
            ApiKeyConfigured = apiKeyConfigured,
            EnableFallback = options.EnableFallback,
            EffectiveBehavior = DescribeEffectiveBehavior(provider, apiKeyConfigured, options.EnableFallback, isDevelopment, isTesting),
            ChatSystemPromptSource = string.IsNullOrWhiteSpace(options.ChatSystemPrompt)
                ? "embedded:chat-sarkastik-dahi.md"
                : "custom:Ai:ChatSystemPrompt",
        };
    }

    private static string DescribeEffectiveBehavior(
        string provider,
        bool apiKeyConfigured,
        bool enableFallback,
        bool isDevelopment,
        bool isTesting)
    {
        if (string.Equals(provider, AiProviders.Fake, StringComparison.OrdinalIgnoreCase))
        {
            return "Fake provider — deterministic AI responses; syllabus uses heuristic parser (no external API).";
        }

        if (apiKeyConfigured)
        {
            return $"Live {provider} API calls enabled for chat, study plan, task feedback, and syllabus AI parsing.";
        }

        if ((isDevelopment || isTesting) && enableFallback)
        {
            return
                "No API key — real AI HTTP calls are skipped where fallbacks exist " +
                "(daily message template, heuristic syllabus parse). " +
                "Chat, study plan, and task feedback will fail at request time with AI_CONFIG until a key is set.";
        }

        if ((isDevelopment || isTesting) && !enableFallback)
        {
            return
                "No API key and EnableFallback=false — all AI endpoints will fail at request time with AI_CONFIG.";
        }

        return "No API key — AI provider calls require Ai:ApiKey (or AI_API_KEY / GEMINI_API_KEY).";
    }
}
