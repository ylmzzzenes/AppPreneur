using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace UniFlow.Business.Configuration.Validators;

public sealed class UniFlowOcrOptionsValidator(IHostEnvironment hostEnvironment) : IValidateOptions<UniFlowOcrOptions>
{
    public ValidateOptionsResult Validate(string? name, UniFlowOcrOptions options)
    {
        if (hostEnvironment.IsDevelopment())
        {
            return ValidateOptionsResult.Success;
        }

        if (options.Provider != OcrProvider.Azure)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.Azure.Endpoint))
        {
            return ValidateOptionsResult.Fail(
                "UniFlow:Ocr:Azure:Endpoint is required when OCR provider is Azure in non-Development environments.");
        }

        if (string.IsNullOrWhiteSpace(options.Azure.ApiKey))
        {
            return ValidateOptionsResult.Fail(
                "UniFlow:Ocr:Azure:ApiKey is required when OCR provider is Azure in non-Development environments. " +
                "Configure via user-secrets or UniFlow__Ocr__Azure__ApiKey.");
        }

        return ValidateOptionsResult.Success;
    }
}
