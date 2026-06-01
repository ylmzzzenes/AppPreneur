using FluentValidation;
using UniFlow.Business.Contracts.Ai;

namespace UniFlow.Business.Validation;

public sealed class StudyPlanRequestValidator : AbstractValidator<StudyPlanRequest>
{
    public StudyPlanRequestValidator()
    {
        RuleFor(x => x.Days).InclusiveBetween(1, 14);
        RuleFor(x => x.Focus).MaximumLength(200).When(x => x.Focus is not null);
    }
}
