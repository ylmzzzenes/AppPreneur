using UniFlow.Business.Contracts.Ai;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

public interface IWeeklySummaryService
{
    Task<Result<WeeklySummaryResponse>> GetAsync(
        long userId,
        CancellationToken cancellationToken = default);
}
