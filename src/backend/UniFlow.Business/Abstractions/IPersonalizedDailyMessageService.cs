using UniFlow.Business.Contracts.Ai;
using UniFlow.Business.Contracts.Dashboard;
using UniFlow.Entity.ReadModels;

namespace UniFlow.Business.Abstractions;

public interface IPersonalizedDailyMessageService
{
    Task<string> BuildDailyMessageAsync(
        DailyMessageContext context,
        AiUserProfileContext? profile,
        CancellationToken cancellationToken = default);
}
