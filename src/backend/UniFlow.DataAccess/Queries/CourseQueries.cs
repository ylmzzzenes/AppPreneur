using Microsoft.EntityFrameworkCore;
using UniFlow.DataAccess.Persistence;
using UniFlow.Entity.Entities;

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
        var code = courseCode.Trim();
        return _dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Code == code, cancellationToken);
    }

    public Task<Course?> GetOwnedAsync(long courseId, long userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId && c.UserId == userId, cancellationToken);
    }
}
