namespace UniFlow.Mobile.Models;

public sealed class TaskListResponseDto
{
    public List<TaskItemResponseDto> Items { get; set; } = [];

    public int PendingCount { get; set; }

    public int DoneCount { get; set; }
}
