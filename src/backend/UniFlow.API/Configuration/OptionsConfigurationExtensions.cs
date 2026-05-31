using Microsoft.Extensions.Options;
using UniFlow.Business.Configuration;
using UniFlow.Business.Configuration.Validators;
using UniFlow.Business.Options;

namespace UniFlow.API.Configuration;

public static class OptionsConfigurationExtensions
{
    public static IServiceCollection AddUniFlowValidatedOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .PostConfigure(options => ApplyJwtSecrets(configuration, options))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<AiOptions>, AiOptionsValidator>();
        services.AddOptions<AiOptions>()
            .Bind(configuration.GetSection(AiOptions.SectionName))
            .PostConfigure(options => ApplyAiSecrets(configuration, options))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<UniFlowGeminiOptions>, UniFlowGeminiOptionsValidator>();
        services.AddOptions<UniFlowGeminiOptions>()
            .Bind(configuration.GetSection(UniFlowGeminiOptions.SectionName))
            .PostConfigure(options => ApplyGeminiSecrets(configuration, options))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<UniFlowOcrOptions>, UniFlowOcrOptionsValidator>();
        services.AddOptions<UniFlowOcrOptions>()
            .Bind(configuration.GetSection(UniFlowOcrOptions.SectionName))
            .PostConfigure(options => ApplyOcrSecrets(configuration, options))
            .ValidateOnStart();

        services.AddOptions<SyllabusTextStorageOptions>()
            .Bind(configuration.GetSection(SyllabusTextStorageOptions.SectionName))
            .ValidateOnStart();

        return services;
    }

    public static void ValidateStartupSecrets(WebApplication app)
    {
        if (app.Environment.IsEnvironment("Testing"))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("DefaultConnection")))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not configured. " +
                "Set ConnectionStrings__DefaultConnection or use user-secrets.");
        }
    }

    private static void ApplyJwtSecrets(IConfiguration configuration, JwtOptions options)
    {
        var key = configuration["JWT_KEY"];
        if (!string.IsNullOrWhiteSpace(key))
        {
            options.Key = key;
        }
    }

    private static void ApplyGeminiSecrets(IConfiguration configuration, UniFlowGeminiOptions options)
    {
        var apiKey = configuration["GEMINI_API_KEY"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            options.ApiKey = apiKey;
        }
    }

    private static void ApplyAiSecrets(IConfiguration configuration, AiOptions options)
    {
        var apiKey = configuration["AI_API_KEY"] ?? configuration["GEMINI_API_KEY"];
        if (!string.IsNullOrWhiteSpace(apiKey) && string.IsNullOrWhiteSpace(options.ApiKey))
        {
            options.ApiKey = apiKey;
        }

        var geminiSection = configuration.GetSection(UniFlowGeminiOptions.SectionName);
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            var legacyKey = geminiSection["ApiKey"];
            if (!string.IsNullOrWhiteSpace(legacyKey))
            {
                options.ApiKey = legacyKey;
            }
        }

        if (string.IsNullOrWhiteSpace(options.Model))
        {
            var legacyModel = geminiSection["Model"];
            if (!string.IsNullOrWhiteSpace(legacyModel))
            {
                options.Model = legacyModel;
            }
        }

        var legacyTimeout = geminiSection.GetValue<int?>("TimeoutSeconds");
        if (legacyTimeout is > 0 && options.TimeoutSeconds == 30)
        {
            options.TimeoutSeconds = legacyTimeout.Value;
        }
    }

    private static void ApplyOcrSecrets(IConfiguration configuration, UniFlowOcrOptions options)
    {
        var azureKey = configuration["AZURE_DOCUMENT_INTELLIGENCE_KEY"];
        if (!string.IsNullOrWhiteSpace(azureKey))
        {
            options.Azure.ApiKey = azureKey;
        }
    }
}
