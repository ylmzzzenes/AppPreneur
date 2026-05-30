namespace UniFlow.Business.Contracts.Tasks;

public sealed class TaskListResponse
{
    public IReadOnlyList<TaskItemResponse> Items { get; set; } = Array.Empty<TaskItemResponse>();

    public int PendingCount { get; set; }

    public int DoneCount { get; set; }
}
