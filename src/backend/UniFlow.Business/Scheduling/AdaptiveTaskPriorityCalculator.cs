using UniFlow.Business.Abstractions;
using UniFlow.Business.Dtos;

namespace UniFlow.Business.Scheduling;

/// <summary>
/// Combines due-date urgency, category weight, and subjective difficulty into a 1–100 score.
/// </summary>
public sealed class AdaptiveTaskPriorityCalculator : ITaskPriorityCalculator
{
    public int CalculateScore(TaskSchedulingInput input)
    {
        var difficulty = Math.Clamp(input.Difficulty, 1, 5);
        var referenceDay = input.ReferenceUtc.Date;

        double urgency;
        if (input.DueDate is null)
        {
            urgency = 42;
        }
        else
        {
            var dueDay = input.DueDate.Value.Date;
            var days = (dueDay - referenceDay).TotalDays;
            urgency = days switch
            {
                < 0 => 100,
                0 => 96,
                <= 1 => 90,
                <= 3 => 82,
                <= 7 => 70,
                <= 14 => 58,
                <= 30 => 48,
                _ => Math.Max(22, 52 - days * 0.35)
            };
        }

        var categoryFactor = GetCategoryFactor(input.Category);
        var difficultyBoost = (difficulty - 3) * 7;

        var raw = urgency * (0.52 + 0.48 * categoryFactor) + difficultyBoost;
        return (int)Math.Clamp(Math.Round(raw), 1, 100);
    }

    private static double GetCategoryFactor(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return 0.72;
        }

        return category.Trim().ToUpperInvariant() switch
        {
            "FINAL" => 1.0,
            "MIDTERM" => 0.9,
            "QUIZ" => 0.78,
            "HOMEWORK" => 0.62,
            "PROJECT" => 0.68,
            "OTHER" => 0.7,
            _ => 0.72
        };
    }
}
