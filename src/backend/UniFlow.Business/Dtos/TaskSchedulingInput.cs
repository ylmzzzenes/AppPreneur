using UniFlow.Entity.Enums;

namespace UniFlow.Business.Dtos;

public sealed class TaskSchedulingInput
{
    public DateTime? DueDate { get; init; }

    public string? Category { get; init; }

    /// <summary>
    /// When <see cref="TaskItemStatus.Done"/>, priority is minimized so completed work does not dominate rankings.
    /// </summary>
    public TaskItemStatus? Status { get; init; }

    /// <summary>
    /// Subjective difficulty 1 (easy) to 5 (hard). Default 3.
    /// </summary>
    public int Difficulty { get; init; } = 3;

    /// <summary>
    /// Reference instant for "days until due" (typically UTC now).
    /// </summary>
    public DateTime ReferenceUtc { get; init; } = DateTime.UtcNow;
}
