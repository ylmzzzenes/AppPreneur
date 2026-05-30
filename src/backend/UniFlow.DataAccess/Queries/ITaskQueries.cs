using UniFlow.Entity.Entities;
using UniFlow.Entity.Enums;
using UniFlow.Entity.ReadModels;

public interface ITaskQueries
{
    Task<IReadOnlyList<TaskItemSummary>> ListForUserAsync(long userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskItemSummary>> ListTodayForUserAsync(long userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskItemSummary>> ListUpcomingForUserAsync(
        long userId,
        int days,
        TaskItemStatus? status,
        CancellationToken cancellationToken = default);

    Task<TaskItem?> GetOwnedAsync(long taskId, long userId, CancellationToken cancellationToken = default);

    Task<TaskItem?> GetOwnedForUpdateAsync(long taskId, long userId, CancellationToken cancellationToken = default);

    Task<Syllabus?> GetManualSyllabusForCourseAsync(long courseId, CancellationToken cancellationToken = default);

    Task<Syllabus?> GetManualSyllabusForCourseForUpdateAsync(long courseId, CancellationToken cancellationToken = default);
}
