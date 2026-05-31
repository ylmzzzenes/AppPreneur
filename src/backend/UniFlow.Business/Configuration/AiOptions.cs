using System.ComponentModel.DataAnnotations;
using UniFlow.Business.Ai;

namespace UniFlow.Business.Configuration;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string Provider { get; set; } = AiProviders.Gemini;

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    [Required]
    public string Model { get; set; } = "gemini-2.0-flash";

    [Range(5, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    [Range(0, 10)]
    public int RetryCount { get; set; } = 2;

    public string PromptVersion { get; set; } = "v1";

    public bool EnableFallback { get; set; } = true;

    public bool LogMetadataOnly { get; set; } = true;
}
