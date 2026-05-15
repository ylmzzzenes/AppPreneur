using FluentValidation;
using UniFlow.Business.Contracts.Tasks;

namespace UniFlow.Business.Validation;

public sealed class TaskStatusUpdateRequestValidator : AbstractValidator<TaskStatusUpdateRequest>
{
    public TaskStatusUpdateRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be one of: Pending, Done, Missed.");
    }
}
