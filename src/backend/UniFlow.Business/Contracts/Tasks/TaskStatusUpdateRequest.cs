using UniFlow.Entity.Enums;

namespace UniFlow.Business.Contracts.Tasks;

public sealed class TaskStatusUpdateRequest
{
    public TaskItemStatus Status { get; set; }
}
