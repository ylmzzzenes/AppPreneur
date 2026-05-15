using FluentValidation;
using UniFlow.Business.Contracts.Users;
using UniFlow.Entity.Enums;

namespace UniFlow.Business.Validation;

public sealed class OnboardingUpdateRequestValidator : AbstractValidator<OnboardingUpdateRequest>
{
    public OnboardingUpdateRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.PersonalityVibe.HasValue || x.Major is not null)
            .WithMessage("At least one of personalityVibe or major must be provided.");

        When(x => x.PersonalityVibe.HasValue, () =>
        {
            RuleFor(x => x.PersonalityVibe)
                .Must(v => Enum.IsDefined(typeof(PersonalityVibe), v!.Value))
                .WithMessage("PersonalityVibe must be a valid value.");
        });

        RuleFor(x => x.Major)
            .MaximumLength(100)
            .When(x => x.Major is not null);
    }
}
