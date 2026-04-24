using UniFlow.Business.Contracts.Tasks;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

public interface ITaskService
{
    Task<Result<IReadOnlyList<TaskItemResponse>>> GetMyTasksAsync(long userId, CancellationToken cancellationToken = default);

    Task<Result<TaskItemResponse>> GetMyTaskAsync(long userId, long taskId, CancellationToken cancellationToken = default);
}
