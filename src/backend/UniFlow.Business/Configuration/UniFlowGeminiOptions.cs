using System.ComponentModel.DataAnnotations;

namespace UniFlow.Business.Configuration;

public sealed class UniFlowGeminiOptions
{
    public const string SectionName = "UniFlow:Gemini";

    /// <summary>
    /// Gemini API key. Configure via user-secrets, UniFlow__Gemini__ApiKey, or GEMINI_API_KEY.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model id without the "models/" prefix, e.g. gemini-2.0-flash.
    /// </summary>
    [Required]
    public string Model { get; set; } = "gemini-2.0-flash";

    /// <summary>
    /// HTTP timeout for Gemini requests (seconds).
    /// </summary>
    [Range(5, 300)]
    public int TimeoutSeconds { get; set; } = 120;
}
