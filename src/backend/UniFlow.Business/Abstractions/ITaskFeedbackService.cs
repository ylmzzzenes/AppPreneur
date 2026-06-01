using UniFlow.Business.Contracts.Ai;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

public interface ITaskFeedbackService
{
    Task<Result<TaskFeedbackResponse>> GenerateAsync(
        long userId,
        TaskFeedbackRequest request,
        CancellationToken cancellationToken = default);
}
