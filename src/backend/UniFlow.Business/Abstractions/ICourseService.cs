using UniFlow.Business.Contracts.Courses;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Abstractions;

public interface ICourseService
{
    Task<Result<IReadOnlyList<CourseResponse>>> ListAsync(long userId, CancellationToken cancellationToken = default);

    Task<Result<CourseResponse>> GetAsync(long userId, long courseId, CancellationToken cancellationToken = default);

    Task<Result<CourseResponse>> CreateAsync(long userId, CreateCourseRequest request, CancellationToken cancellationToken = default);

    Task<Result<CourseResponse>> UpdateAsync(
        long userId,
        long courseId,
        UpdateCourseRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> DeleteAsync(long userId, long courseId, CancellationToken cancellationToken = default);
}
