using System.Globalization;
using System.Text;
using UniFlow.DataAccess.Queries;
using UniFlow.Entity.Enums;
using UniFlow.Entity.ReadModels;

namespace UniFlow.Business.Services;

public static class ChatUserContextBuilder
{
    internal const int MaxTasksInContext = 40;

    public static async Task<string> BuildAsync(
        long userId,
        IUserQueries userQueries,
        ICourseQueries courseQueries,
        ITaskQueries taskQueries,
        CancellationToken cancellationToken)
    {
        var profile = await userQueries.GetAiProfileContextAsync(userId, cancellationToken).ConfigureAwait(false);
        var courses = await courseQueries.ListForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        var tasks = await taskQueries.ListForUserAsync(userId, cancellationToken).ConfigureAwait(false);

        var sb = new StringBuilder();
        sb.AppendLine("[UniFlow kullanıcı özeti — yanıtında buna dayanabilirsin]");

        if (profile is not null)
        {
            sb.Append("Profil: ");
            if (!string.IsNullOrWhiteSpace(profile.DisplayName))
            {
                sb.Append(profile.DisplayName.Trim());
            }

            if (!string.IsNullOrWhiteSpace(profile.Major))
            {
                sb.Append(CultureInfo.InvariantCulture, $", bölüm: {profile.Major.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(profile.AcademicGoal))
            {
                sb.Append(CultureInfo.InvariantCulture, $", hedef: {profile.AcademicGoal.Trim()}");
            }

            sb.AppendLine();
        }

        sb.AppendLine("Dersler:");
        if (courses.Count == 0)
        {
            sb.AppendLine("- (henüz ders yok)");
        }
        else
        {
            foreach (var course in courses)
            {
                sb.AppendLine(CultureInfo.InvariantCulture,
                    $"- {course.Code} — {course.Title} | aktif görev: {course.ActiveTaskCount}, toplam: {course.TaskCount}");
            }
        }

        var selectedTasks = SelectTasksForContext(tasks);
        sb.AppendLine("Görevler:");
        if (selectedTasks.Count == 0)
        {
            sb.AppendLine("- (henüz görev yok)");
        }
        else
        {
            foreach (var task in selectedTasks)
            {
                sb.AppendLine(FormatTaskLine(task));
            }

            if (tasks.Count > selectedTasks.Count)
            {
                sb.AppendLine(CultureInfo.InvariantCulture,
                    $"- … ve {tasks.Count - selectedTasks.Count} görev daha (özet kısaltıldı)");
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static IReadOnlyList<TaskItemSummary> SelectTasksForContext(IReadOnlyList<TaskItemSummary> tasks)
    {
        if (tasks.Count <= MaxTasksInContext)
        {
            return tasks;
        }

        var today = DateTime.UtcNow.Date;
        return tasks
            .OrderBy(t => t.Status == TaskItemStatus.Pending ? 0 : t.Status == TaskItemStatus.Missed ? 1 : 2)
            .ThenByDescending(t => t.PriorityScore ?? 0)
            .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
            .Take(MaxTasksInContext)
            .ToList();
    }

    private static string FormatTaskLine(TaskItemSummary task)
    {
        var due = task.DueDate.HasValue
            ? task.DueDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : "tarih yok";
        var priority = task.PriorityScore?.ToString(CultureInfo.InvariantCulture) ?? "-";
        return $"- [{FormatStatus(task.Status)}] {task.Title} | {task.CourseCode} | son tarih: {due} | öncelik: {priority}";
    }

    private static string FormatStatus(TaskItemStatus status) =>
        status switch
        {
            TaskItemStatus.Pending => "Bekliyor",
            TaskItemStatus.Done => "Tamamlandı",
            TaskItemStatus.Missed => "Kaçırıldı",
            _ => status.ToString(),
        };
}
