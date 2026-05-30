using UniFlow.Business.Contracts.Tasks;
using UniFlow.Entity.Enums;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

public interface ITaskService
{
    Task<Result<IReadOnlyList<TaskItemResponse>>> GetMyTasksAsync(long userId, CancellationToken cancellationToken = default);

    Task<Result<TaskListResponse>> GetTodayTasksAsync(long userId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TaskItemResponse>>> GetUpcomingTasksAsync(
        long userId,
        int days,
        TaskItemStatus? status,
        CancellationToken cancellationToken = default);

    Task<Result<TaskItemResponse>> GetMyTaskAsync(long userId, long taskId, CancellationToken cancellationToken = default);

    Task<Result<TaskItemResponse>> CreateAsync(long userId, CreateTaskRequest request, CancellationToken cancellationToken = default);

    Task<Result<TaskItemResponse>> UpdateAsync(
        long userId,
        long taskId,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<TaskItemResponse>> UpdateStatusAsync(
        long userId,
        long taskId,
        TaskStatusUpdateRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> DeleteAsync(long userId, long taskId, CancellationToken cancellationToken = default);
}
