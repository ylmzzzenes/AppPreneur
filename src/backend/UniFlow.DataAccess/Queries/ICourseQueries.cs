using UniFlow.Entity.Entities;

namespace UniFlow.DataAccess.Queries;

public interface ICourseQueries
{
    Task<Course?> FindByUserAndCodeAsync(long userId, string courseCode, CancellationToken cancellationToken = default);

    Task<Course?> GetOwnedAsync(long courseId, long userId, CancellationToken cancellationToken = default);
}
