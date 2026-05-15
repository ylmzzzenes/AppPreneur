using UniFlow.Business.Contracts.Dashboard;

namespace UniFlow.Business.Abstractions;

public interface IDailyMessageService
{
    string BuildDailyMessage(DailyMessageContext context);
}
