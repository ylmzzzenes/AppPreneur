namespace UniFlow.Business.Ai;

public static class AiProviders
{
    public const string Gemini = "Gemini";

    public const string OpenAiCompatible = "OpenAiCompatible";

    public const string Fake = "Fake";

    public static bool IsKnown(string? provider) =>
        string.Equals(provider, Gemini, StringComparison.OrdinalIgnoreCase)
        || string.Equals(provider, OpenAiCompatible, StringComparison.OrdinalIgnoreCase)
        || string.Equals(provider, Fake, StringComparison.OrdinalIgnoreCase);
}
