using UniFlow.Business.Abstractions;
using UniFlow.Business.Dtos;

namespace UniFlow.Business.Scheduling;

public static class TaskDraftSchedulingExtensions
{
    /// <summary>
    /// Fills <see cref="SyllabusTaskDraft.PriorityScore"/> for each draft using the adaptive calculator.
    /// </summary>
    public static void ApplyPriorityScores(
        this IList<SyllabusTaskDraft> drafts,
        ITaskPriorityCalculator calculator,
        DateTime? referenceUtc = null,
        int defaultDifficulty = 3)
    {
        var reference = referenceUtc ?? DateTime.UtcNow;
        foreach (var draft in drafts)
        {
            draft.PriorityScore = calculator.CalculateScore(new TaskSchedulingInput
            {
                DueDate = draft.DueDate,
                Category = draft.Category,
                Difficulty = defaultDifficulty,
                ReferenceUtc = reference,
            });
        }
    }
}
