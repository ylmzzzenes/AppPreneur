using UniFlow.Business.Dtos;

namespace UniFlow.Business.Abstractions;

public interface ITaskPriorityCalculator
{
    /// <summary>
    /// Returns a priority score between 1 and 100 (inclusive).
    /// </summary>
    int CalculateScore(TaskSchedulingInput input);
}
