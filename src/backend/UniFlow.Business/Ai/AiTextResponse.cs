namespace UniFlow.Business.Ai;

public sealed class AiTextResponse
{
    public string Content { get; init; } = string.Empty;

    public string Provider { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public string PromptVersion { get; init; } = string.Empty;

    public bool IsFallback { get; init; }

    public int? InputTokenEstimate { get; init; }

    public int? OutputTokenEstimate { get; init; }
}
