using FluentValidation;
using UniFlow.Business.Contracts.Auth;
using UniFlow.Entity.Enums;

namespace UniFlow.Business.Validation;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(200);

        When(x => x.PersonalityVibe.HasValue, () =>
        {
            RuleFor(x => x.PersonalityVibe)
                .Must(v => Enum.IsDefined(typeof(PersonalityVibe), v!.Value))
                .WithMessage("PersonalityVibe must be a valid value.");
        });

        RuleFor(x => x.Major)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Major));
    }
}
