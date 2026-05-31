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
        if (request.Metadata is not null
            && request.Metadata.TryGetValue("kind", out var kind)
            && string.Equals(kind, "chat", StringComparison.OrdinalIgnoreCase))
        {
            return "Fake assistant reply for local testing.";
        }

        return "[Fake AI] Deterministic response for testing.";
    }
}
