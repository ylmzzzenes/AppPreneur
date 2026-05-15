using UniFlow.Entity.ReadModels;

namespace UniFlow.DataAccess.Queries;

public interface IDashboardQueries
{
    Task<IReadOnlyList<DashboardTaskRow>> ListTaskRowsForUserAsync(long userId, CancellationToken cancellationToken = default);
}
