using Microsoft.Extensions.Logging;

namespace UniFlow.Business.Ai;

internal static class AiRequestLogger
{
    public static void LogCompleted(ILogger logger, AiTextResponse response, int inputLength)
    {
        logger.LogInformation(
            "AI request completed. Provider={Provider}, Model={Model}, PromptVersion={PromptVersion}, InputLength={InputLength}, OutputLength={OutputLength}, IsFallback={IsFallback}",
            response.Provider,
            response.Model,
            response.PromptVersion,
            inputLength,
            response.Content.Length,
            response.IsFallback);
    }

    public static void LogFailed(ILogger logger, string provider, string code, int? statusCode = null)
    {
        if (statusCode.HasValue)
        {
            logger.LogWarning(
                "AI request failed. Provider={Provider}, Code={Code}, StatusCode={StatusCode}",
                provider,
                code,
                statusCode.Value);
            return;
        }

        logger.LogWarning(
            "AI request failed. Provider={Provider}, Code={Code}",
            provider,
            code);
    }
}
