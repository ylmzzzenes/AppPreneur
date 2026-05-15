using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace UniFlow.Business.Configuration.Validators;

public sealed class UniFlowGeminiOptionsValidator(IHostEnvironment hostEnvironment) : IValidateOptions<UniFlowGeminiOptions>
{
    public ValidateOptionsResult Validate(string? name, UniFlowGeminiOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Model))
        {
            return ValidateOptionsResult.Fail("UniFlow:Gemini:Model is required.");
        }

        if (options.TimeoutSeconds is < 5 or > 300)
        {
            return ValidateOptionsResult.Fail("UniFlow:Gemini:TimeoutSeconds must be between 5 and 300.");
        }

        if (!hostEnvironment.IsDevelopment() && string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return ValidateOptionsResult.Fail(
                "UniFlow:Gemini:ApiKey is required outside Development. " +
                "Configure via user-secrets, UniFlow__Gemini__ApiKey, or GEMINI_API_KEY.");
        }

        return ValidateOptionsResult.Success;
    }
}
