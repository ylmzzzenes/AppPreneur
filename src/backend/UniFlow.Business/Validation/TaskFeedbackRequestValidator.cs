using FluentValidation;
using UniFlow.Business.Contracts.Ai;

namespace UniFlow.Business.Validation;

public sealed class TaskFeedbackRequestValidator : AbstractValidator<TaskFeedbackRequest>
{
    public TaskFeedbackRequestValidator()
    {
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}
