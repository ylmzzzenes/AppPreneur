using UniFlow.Entity.Enums;

namespace UniFlow.Business.Contracts.Ai;

public sealed class TaskFeedbackRequest
{
    public long TaskId { get; init; }

    public TaskItemStatus NewStatus { get; init; }
}
