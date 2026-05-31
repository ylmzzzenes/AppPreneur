namespace UniFlow.Business.Ai;

internal static class AiTokenEstimator
{
    public static int Estimate(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        return Math.Max(1, text.Length / 4);
    }

    public static int Estimate(AiTextRequest request) =>
        Estimate(request.SystemPrompt) + Estimate(request.UserPrompt);
}
