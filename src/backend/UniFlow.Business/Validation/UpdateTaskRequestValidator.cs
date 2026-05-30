using FluentValidation;
using UniFlow.Business.Contracts.Tasks;

namespace UniFlow.Business.Validation;

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);

        RuleFor(x => x.EstimatedMinutes)
            .InclusiveBetween(0, 1440)
            .When(x => x.EstimatedMinutes.HasValue);

        RuleFor(x => x.PriorityScore)
            .InclusiveBetween(0, 100)
            .When(x => x.PriorityScore.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be one of: Pending, Done, Missed.");
    }
}
