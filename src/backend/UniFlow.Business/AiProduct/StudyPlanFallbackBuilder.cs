using System.Text.Json;
using UniFlow.Business.Contracts.Ai;
using UniFlow.Entity.Enums;
using UniFlow.Entity.ReadModels;

namespace UniFlow.Business.AiProduct;

internal static class StudyPlanFallbackBuilder
{
    internal const int MaxTasks = 30;

    internal static StudyPlanResponse Build(
        IReadOnlyList<TaskItemSummary> tasks,
        int days,
        string? focus,
        DateTime startDate)
    {
        var eligible = tasks
            .Where(t => t.Status != TaskItemStatus.Done)
            .OrderByDescending(t => t.PriorityScore ?? 0)
            .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
            .Take(MaxTasks)
            .ToList();

        var planDays = new List<StudyPlanDayResponse>();
        var perDay = Math.Max(1, (int)Math.Ceiling(eligible.Count / (double)Math.Max(1, days)));

        for (var d = 0; d < days; d++)
        {
            var date = startDate.AddDays(d).ToString("yyyy-MM-dd");
            var slice = eligible.Skip(d * perDay).Take(perDay).ToList();
            if (slice.Count == 0 && d == 0 && eligible.Count == 0)
            {
                slice = [];
            }

            planDays.Add(new StudyPlanDayResponse
            {
                Date = date,
                Focus = d == 0
                    ? focus ?? "Öncelikli görevlere odaklan"
                    : $"Gün {d + 1} çalışması",
                Tip = "Kısa molalar ver ve tek seferde bir göreve odaklan.",
                Tasks = slice.Select(t => new StudyPlanTaskResponse
                {
                    Title = t.Title,
                    EstimatedMinutes = t.EstimatedMinutes ?? 30,
                    Reason = t.PriorityScore is > 70 ? "Yüksek öncelik" : "Yaklaşan teslim",
                }).ToList(),
            });
        }

        return new StudyPlanResponse
        {
            Title = focus is not null ? $"{focus} — {days} günlük plan" : $"{days} günlük çalışma planı",
            Summary = eligible.Count > 0
                ? $"{eligible.Count} görev {days} güne dağıtıldı."
                : "Henüz bekleyen görev yok; plan boş günlerle oluşturuldu.",
            Days = planDays,
            IsFallback = true,
        };
    }

    internal static string BuildTasksJson(IReadOnlyList<TaskItemSummary> tasks) =>
        JsonSerializer.Serialize(tasks.Select(t => new
        {
            courseCode = t.CourseCode,
            courseTitle = t.CourseTitle,
            title = t.Title,
            dueDate = t.DueDate?.ToString("yyyy-MM-dd"),
            status = t.Status.ToString(),
            priorityScore = t.PriorityScore,
            estimatedMinutes = t.EstimatedMinutes,
        }));
}
