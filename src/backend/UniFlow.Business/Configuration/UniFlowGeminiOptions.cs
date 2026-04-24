namespace UniFlow.Business.Configuration;

public sealed class UniFlowGeminiOptions
{
    public const string SectionName = "UniFlow:Gemini";

    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model id without the "models/" prefix, e.g. gemini-2.0-flash or gemini-1.5-flash.
    /// </summary>
    public string Model { get; set; } = "gemini-2.0-flash";
}
