using UniFlow.Entity.Entities;
using UniFlow.Entity.ReadModels;

namespace UniFlow.DataAccess.Queries;

public interface ITaskQueries
{
    Task<IReadOnlyList<TaskItemSummary>> ListForUserAsync(long userId, CancellationToken cancellationToken = default);

    Task<TaskItem?> GetOwnedAsync(long taskId, long userId, CancellationToken cancellationToken = default);

    Task<TaskItem?> GetOwnedForUpdateAsync(long taskId, long userId, CancellationToken cancellationToken = default);
}
