using Microsoft.Extensions.Options;
using UniFlow.Business.Ai;
using UniFlow.Business.Configuration;

namespace UniFlow.API.Configuration;

public static class AiStartupDiagnosticsExtensions
{
    public static void LogAiStartupDiagnostics(this WebApplication app)
    {
        if (app.Environment.IsEnvironment("Testing"))
        {
            return;
        }

        var ai = app.Services.GetRequiredService<IOptions<AiOptions>>().Value;
        var snapshot = AiConfigurationDiagnostics.Create(app.Environment.EnvironmentName, ai);
        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("UniFlow.Ai.Configuration");

        logger.LogInformation(
            "AI configuration: Environment={Environment}, Provider={Provider}, Model={Model}, ApiKeyConfigured={ApiKeyConfigured}, EnableFallback={EnableFallback}, ChatSystemPromptSource={ChatSystemPromptSource}, EffectiveBehavior={EffectiveBehavior}",
            snapshot.Environment,
            snapshot.Provider,
            snapshot.Model,
            snapshot.ApiKeyConfigured,
            snapshot.EnableFallback,
            snapshot.ChatSystemPromptSource,
            snapshot.EffectiveBehavior);

        if (snapshot.BaseUrl is not null)
        {
            logger.LogInformation("AI OpenAiCompatible BaseUrl={BaseUrl}", snapshot.BaseUrl);
        }

        if (!snapshot.ApiKeyConfigured
            && !string.Equals(snapshot.Provider, AiProviders.Fake, StringComparison.OrdinalIgnoreCase)
            && app.Environment.IsDevelopment())
        {
            logger.LogDebug(
                "AI development mode without API key: configure Ai:ApiKey, Ai__ApiKey, AI_API_KEY, or GEMINI_API_KEY " +
                "to enable live {Provider} calls. Set Ai:Provider=Fake for keyless end-to-end demo.",
                snapshot.Provider);
        }
    }
}
