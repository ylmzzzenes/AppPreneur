using UniFlow.Business.Contracts.Dashboard;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

public interface IDashboardService
{
    Task<Result<DashboardTodayResponse>> GetTodayAsync(long userId, CancellationToken cancellationToken = default);
}
