using Microsoft.Extensions.Options;
using UniFlow.Business.Configuration;

namespace UniFlow.Business.Ai.Providers;

public sealed class FakeAiProvider(IOptions<AiOptions> options)
{
    public Task<AiTextResponse> GenerateTextAsync(
        AiTextRequest request,
        CancellationToken cancellationToken = default)
    {
        var inputEstimate = AiTokenEstimator.Estimate(request);
        var content = BuildDeterministicContent(request);
        var response = new AiTextResponse
        {
            Content = content,
            Provider = AiProviders.Fake,
            Model = "fake-model",
            PromptVersion = request.PromptVersion ?? options.Value.PromptVersion,
            IsFallback = true,
            InputTokenEstimate = inputEstimate,
            OutputTokenEstimate = AiTokenEstimator.Estimate(content),
        };

        return Task.FromResult(response);
    }

    private static string BuildDeterministicContent(AiTextRequest request)
    {
        var kind = request.Metadata?.GetValueOrDefault("kind") ?? string.Empty;

        return kind.ToLowerInvariant() switch
        {
            "chat" => "Fake assistant reply for local testing.",
            "study-plan" => """
                {
                  "title": "Fake Study Plan",
                  "summary": "Deterministic 7-day plan for testing.",
                  "days": [
                    {
                      "date": "2026-06-01",
                      "focus": "Öncelikli görevler",
                      "tasks": [
                        { "title": "Fake Task A", "estimatedMinutes": 45, "reason": "Yüksek öncelik" }
                      ],
                      "tip": "Kısa molalar ver."
                    }
                  ]
                }
                """,
            "task-feedback" => """
                {
                  "message": "Güzel ilerleme! Bir sonraki göreve geçebilirsin.",
                  "tone": "Motivational",
                  "nextAction": "Big 3 listendeki sıradaki göreve bak."
                }
                """,
            "daily-message" => "Bugün odaklı kal — öncelikli görevlerine küçük adımlarla başla.",
            "weekly-summary" => """
                {
                  "summary": "Bu hafta istikrarlı bir tempo yakaladın.",
                  "strongPoint": "Düzenli takip",
                  "improvementPoint": "Erken başlama alışkanlığı",
                  "nextWeekFocus": "Yüksek öncelikli görevler"
                }
                """,
            _ => "[Fake AI] Deterministic response for testing.",
        };
    }
}
