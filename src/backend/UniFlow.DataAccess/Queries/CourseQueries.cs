using Microsoft.EntityFrameworkCore;
using UniFlow.DataAccess.Persistence;
using UniFlow.Entity.Entities;
using UniFlow.Entity.Enums;
using UniFlow.Entity.ReadModels;

namespace UniFlow.DataAccess.Queries;

public sealed class CourseQueries : ICourseQueries
{
    private readonly UniFlowDbContext _dbContext;

    public CourseQueries(UniFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Course?> FindByUserAndCodeAsync(long userId, string courseCode, CancellationToken cancellationToken = default)
    {
        var code = courseCode.Trim().ToLowerInvariant();
        return _dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.UserId == userId && c.Code.ToLower() == code,
                cancellationToken);
    }

    public Task<Course?> GetOwnedAsync(long courseId, long userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId && c.UserId == userId, cancellationToken);
    }

    public Task<Course?> GetOwnedForUpdateAsync(long courseId, long userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<CourseSummary>> ListForUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Courses
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Code)
            .Select(c => new CourseSummary
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                Description = c.Description,
                Color = c.Color,
                CreatedDate = c.CreatedDate,
                UpdatedDate = c.UpdatedDate,
                TaskCount = c.Syllabi.SelectMany(s => s.Tasks).Count(),
                ActiveTaskCount = c.Syllabi.SelectMany(s => s.Tasks).Count(t => t.Status == TaskItemStatus.Pending),
            })
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByUserAndCodeAsync(
        long userId,
        string courseCode,
        long? excludeCourseId = null,
        CancellationToken cancellationToken = default)
    {
        var code = courseCode.Trim().ToLowerInvariant();
        var query = _dbContext.Courses
            .AsNoTracking()
            .Where(c => c.UserId == userId && c.Code.ToLower() == code);

        if (excludeCourseId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCourseId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> HasTasksAsync(long courseId, CancellationToken cancellationToken = default)
    {
        return _dbContext.TaskItems
            .AsNoTracking()
            .AnyAsync(t => t.Syllabus.CourseId == courseId, cancellationToken);
    }
}
