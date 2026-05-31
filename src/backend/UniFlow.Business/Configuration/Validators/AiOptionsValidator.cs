using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace UniFlow.Business.Configuration.Validators;

public sealed class AiOptionsValidator(IHostEnvironment hostEnvironment) : IValidateOptions<AiOptions>
{
    public ValidateOptionsResult Validate(string? name, AiOptions options)
    {
        if (!Ai.AiProviders.IsKnown(options.Provider))
        {
            return ValidateOptionsResult.Fail(
                $"Ai:Provider '{options.Provider}' is not supported. Use Gemini, OpenAiCompatible, or Fake.");
        }

        if (string.IsNullOrWhiteSpace(options.Model))
        {
            return ValidateOptionsResult.Fail("Ai:Model is required.");
        }

        if (options.TimeoutSeconds is < 5 or > 300)
        {
            return ValidateOptionsResult.Fail("Ai:TimeoutSeconds must be between 5 and 300.");
        }

        if (options.RetryCount is < 0 or > 10)
        {
            return ValidateOptionsResult.Fail("Ai:RetryCount must be between 0 and 10.");
        }

        if (string.Equals(options.Provider, Ai.AiProviders.OpenAiCompatible, StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            return ValidateOptionsResult.Fail("Ai:BaseUrl is required when Provider is OpenAiCompatible.");
        }

        if (string.Equals(options.Provider, Ai.AiProviders.Fake, StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Success;
        }

        if (!hostEnvironment.IsDevelopment()
            && !hostEnvironment.IsEnvironment("Testing")
            && string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return ValidateOptionsResult.Fail(
                "Ai:ApiKey is required outside Development when Provider is not Fake. " +
                "Configure via Ai__ApiKey, AI_API_KEY, or GEMINI_API_KEY.");
        }

        return ValidateOptionsResult.Success;
    }
}
