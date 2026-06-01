using UniFlow.Business.Contracts.Ai;
using UniFlow.Entity.Enums;
using UniFlow.Entity.ReadModels;

namespace UniFlow.Business.AiProduct;

internal static class WeeklySummaryFallbackBuilder
{
    internal static WeeklySummaryResponse Build(
        IReadOnlyList<TaskItemSummary> weekTasks,
        int completed,
        int missed,
        int pending)
    {
        var topCourse = weekTasks
            .GroupBy(t => t.CourseCode)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault() ?? "derslerin";

        return new WeeklySummaryResponse
        {
            Summary = completed >= missed
                ? $"Bu hafta {completed} görev tamamladın. {pending} görev hâlâ bekliyor."
                : $"Bu hafta {missed} görev kaçırıldı. {completed} tamamlanan var; tempo yükseltilebilir.",
            CompletedCount = completed,
            MissedCount = missed,
            PendingCount = pending,
            StrongPoint = completed > 0 ? "Tamamlanan görevlerde istikrar" : "Planlı takip yapıyorsun",
            ImprovementPoint = missed > 0 ? "Kaçırılan görevler için erken hatırlatıcı kullan" : "Öncelikli görevlere odaklan",
            NextWeekFocus = $"{topCourse} dersine öncelik ver",
            IsFallback = true,
        };
    }

    internal static string BuildTasksSummary(IReadOnlyList<TaskItemSummary> tasks) =>
        string.Join("\n", tasks.Take(20).Select(t =>
            $"- [{t.Status}] {t.CourseCode}: {t.Title} (due {t.DueDate:yyyy-MM-dd}, priority {t.PriorityScore})"));
}
