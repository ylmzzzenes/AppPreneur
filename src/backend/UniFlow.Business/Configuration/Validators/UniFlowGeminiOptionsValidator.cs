using Microsoft.Extensions.Options;

namespace UniFlow.Business.Configuration.Validators;

public sealed class UniFlowGeminiOptionsValidator : IValidateOptions<UniFlowGeminiOptions>
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

        // ApiKey validation is owned by AiOptionsValidator (Ai section is the primary config).
        return ValidateOptionsResult.Success;
    }
}
