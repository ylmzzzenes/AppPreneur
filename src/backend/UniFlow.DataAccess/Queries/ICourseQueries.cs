using UniFlow.Entity.Entities;
using UniFlow.Entity.ReadModels;

namespace UniFlow.DataAccess.Queries;

public interface ICourseQueries
{
    Task<Course?> FindByUserAndCodeAsync(long userId, string courseCode, CancellationToken cancellationToken = default);

    Task<Course?> GetOwnedAsync(long courseId, long userId, CancellationToken cancellationToken = default);

    Task<Course?> GetOwnedForUpdateAsync(long courseId, long userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CourseSummary>> ListForUserAsync(long userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserAndCodeAsync(
        long userId,
        string courseCode,
        long? excludeCourseId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasTasksAsync(long courseId, CancellationToken cancellationToken = default);
}
