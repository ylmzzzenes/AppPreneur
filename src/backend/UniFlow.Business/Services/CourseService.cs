using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Courses;
using UniFlow.Business.Helpers;
using UniFlow.DataAccess.Queries;
using UniFlow.DataAccess.UnitOfWork;
using UniFlow.Entity.Entities;
using UniFlow.Entity.ReadModels;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services;

public sealed class CourseService(IUnitOfWork unitOfWork, ICourseQueries courseQueries) : ICourseService
{
    public async Task<Result<IReadOnlyList<CourseResponse>>> ListAsync(long userId, CancellationToken cancellationToken = default)
    {
        var rows = await courseQueries.ListForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        var list = rows.Select(Map).ToList();
        return Result<IReadOnlyList<CourseResponse>>.Success(list);
    }

    public async Task<Result<CourseResponse>> GetAsync(long userId, long courseId, CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(userId, courseId, cancellationToken).ConfigureAwait(false);
        if (summary is null)
        {
            return Result<CourseResponse>.Fail("COURSE_NOT_FOUND", "Course was not found.");
        }

        return Result<CourseResponse>.Success(Map(summary));
    }

    public async Task<Result<CourseResponse>> CreateAsync(
        long userId,
        CreateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = CourseNormalizer.NormalizeCode(request.Code);
        if (await courseQueries.ExistsByUserAndCodeAsync(userId, code, cancellationToken: cancellationToken)
                .ConfigureAwait(false))
        {
            return Result<CourseResponse>.Fail("COURSE_CODE_DUPLICATE", "A course with this code already exists.");
        }

        var course = new Course
        {
            UserId = userId,
            Code = code,
            Title = CourseNormalizer.NormalizeTitle(request.Title),
            Description = CourseNormalizer.NormalizeOptional(request.Description),
            Color = CourseNormalizer.NormalizeOptional(request.Color),
        };

        unitOfWork.Repository<Course>().Add(course);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var summary = await GetSummaryAsync(userId, course.Id, cancellationToken).ConfigureAwait(false);
        return Result<CourseResponse>.Success(Map(summary!));
    }

    public async Task<Result<CourseResponse>> UpdateAsync(
        long userId,
        long courseId,
        UpdateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        var course = await courseQueries.GetOwnedForUpdateAsync(courseId, userId, cancellationToken).ConfigureAwait(false);
        if (course is null)
        {
            return Result<CourseResponse>.Fail("COURSE_NOT_FOUND", "Course was not found.");
        }

        var code = CourseNormalizer.NormalizeCode(request.Code);
        if (await courseQueries.ExistsByUserAndCodeAsync(userId, code, courseId, cancellationToken).ConfigureAwait(false))
        {
            return Result<CourseResponse>.Fail("COURSE_CODE_DUPLICATE", "A course with this code already exists.");
        }

        course.Code = code;
        course.Title = CourseNormalizer.NormalizeTitle(request.Title);
        course.Description = CourseNormalizer.NormalizeOptional(request.Description);
        course.Color = CourseNormalizer.NormalizeOptional(request.Color);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var summary = await GetSummaryAsync(userId, courseId, cancellationToken).ConfigureAwait(false);
        return Result<CourseResponse>.Success(Map(summary!));
    }

    public async Task<Result<bool>> DeleteAsync(long userId, long courseId, CancellationToken cancellationToken = default)
    {
        var course = await courseQueries.GetOwnedForUpdateAsync(courseId, userId, cancellationToken).ConfigureAwait(false);
        if (course is null)
        {
            return Result<bool>.Fail("COURSE_NOT_FOUND", "Course was not found.");
        }

        if (await courseQueries.HasTasksAsync(courseId, cancellationToken).ConfigureAwait(false))
        {
            return Result<bool>.Fail("COURSE_HAS_TASKS", "Course has tasks and cannot be deleted.");
        }

        unitOfWork.Repository<Course>().Remove(course);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<bool>.Success(true);
    }

    private async Task<CourseSummary?> GetSummaryAsync(long userId, long courseId, CancellationToken cancellationToken)
    {
        var list = await courseQueries.ListForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        return list.FirstOrDefault(c => c.Id == courseId);
    }

    private static CourseResponse Map(CourseSummary s) => new()
    {
        Id = s.Id,
        Code = s.Code,
        Title = s.Title,
        Description = s.Description,
        Color = s.Color,
        CreatedAt = s.CreatedDate,
        UpdatedAt = s.UpdatedDate,
        TaskCount = s.TaskCount,
        ActiveTaskCount = s.ActiveTaskCount,
    };
}
