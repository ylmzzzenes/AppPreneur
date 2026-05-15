using Microsoft.Extensions.Options;
using UniFlow.Business.Configuration;
using UniFlow.Business.Configuration.Validators;

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

        return services;
    }

    public static void ValidateProductionSecrets(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("DefaultConnection")))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not configured. " +
                "Set ConnectionStrings__DefaultConnection for this environment.");
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

    private static void ApplyOcrSecrets(IConfiguration configuration, UniFlowOcrOptions options)
    {
        var azureKey = configuration["AZURE_DOCUMENT_INTELLIGENCE_KEY"];
        if (!string.IsNullOrWhiteSpace(azureKey))
        {
            options.Azure.ApiKey = azureKey;
        }
    }
}
