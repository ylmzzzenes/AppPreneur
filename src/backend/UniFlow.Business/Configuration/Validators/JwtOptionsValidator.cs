using Microsoft.Extensions.Options;

namespace UniFlow.Business.Configuration.Validators;

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            return ValidateOptionsResult.Fail("Jwt:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            return ValidateOptionsResult.Fail("Jwt:Audience is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Key))
        {
            return ValidateOptionsResult.Fail(
                "Jwt:Key is required. Configure via user-secrets, Jwt__Key, or JWT_KEY environment variable.");
        }

        if (options.Key.Length < 32)
        {
            return ValidateOptionsResult.Fail("Jwt:Key must be at least 32 characters.");
        }

        if (options.AccessTokenMinutes is < 1 or > 1440)
        {
            return ValidateOptionsResult.Fail("Jwt:AccessTokenMinutes must be between 1 and 1440.");
        }

        return ValidateOptionsResult.Success;
    }
}
