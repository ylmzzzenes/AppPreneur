namespace UniFlow.Business.Ai;

public sealed class AiTextRequest
{
    public string SystemPrompt { get; init; } = string.Empty;

    public string UserPrompt { get; init; } = string.Empty;

    public string? Model { get; init; }

    public string? PromptVersion { get; init; }

    public double? Temperature { get; init; }

    public int? MaxOutputTokens { get; init; }

    public Dictionary<string, string>? Metadata { get; init; }
}
