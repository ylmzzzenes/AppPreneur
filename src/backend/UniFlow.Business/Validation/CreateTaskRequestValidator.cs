using FluentValidation;
using UniFlow.Business.Contracts.Tasks;
using UniFlow.Entity.Enums;

namespace UniFlow.Business.Validation;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
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

        When(x => x.Status.HasValue, () =>
        {
            RuleFor(x => x.Status)
                .Must(v => Enum.IsDefined(typeof(TaskItemStatus), v!.Value))
                .WithMessage("Status must be one of: Pending, Done, Missed.");
        });
    }
}
