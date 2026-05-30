using FluentValidation;
using UniFlow.Business.Contracts.Users;
using UniFlow.Entity.Enums;

namespace UniFlow.Business.Validation;

public sealed class OnboardingUpdateRequestValidator : AbstractValidator<OnboardingUpdateRequest>
{
    public OnboardingUpdateRequestValidator()
    {
        RuleFor(x => x)
            .Must(HasAtLeastOneField)
            .WithMessage("At least one profile field must be provided.");

        When(x => x.DisplayName is not null, () =>
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .WithMessage("DisplayName cannot be empty.")
                .MaximumLength(100);
        });

        When(x => x.Major is not null, () =>
        {
            RuleFor(x => x.Major)
                .MaximumLength(120);
        });

        When(x => x.AcademicGoal is not null, () =>
        {
            RuleFor(x => x.AcademicGoal)
                .MaximumLength(500);
        });

        When(x => x.PersonalityVibe.HasValue, () =>
        {
            RuleFor(x => x.PersonalityVibe)
                .Must(v => Enum.IsDefined(typeof(PersonalityVibe), v!.Value))
                .WithMessage("PersonalityVibe must be a valid value.");
        });

        When(x => x.DailyStudyTargetMinutes.HasValue, () =>
        {
            RuleFor(x => x.DailyStudyTargetMinutes)
                .InclusiveBetween(0, 720)
                .WithMessage("DailyStudyTargetMinutes must be between 0 and 720.");
        });
    }

    private static bool HasAtLeastOneField(OnboardingUpdateRequest request) =>
        request.DisplayName is not null
        || request.Major is not null
        || request.AcademicGoal is not null
        || request.PersonalityVibe.HasValue
        || request.DailyStudyTargetMinutes.HasValue;
}
