using Microsoft.EntityFrameworkCore;
using UniFlow.DataAccess.Persistence;
using UniFlow.Entity.ReadModels;

namespace UniFlow.DataAccess.Queries;

public sealed class DashboardQueries(UniFlowDbContext dbContext) : IDashboardQueries
{
    public async Task<IReadOnlyList<DashboardTaskRow>> ListTaskRowsForUserAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.TaskItems
            .AsNoTracking()
            .Where(t => t.Syllabus.Course.UserId == userId)
            .Select(t => new DashboardTaskRow
            {
                Id = t.Id,
                Title = t.Title,
                DueDate = t.DueDate,
                Category = t.Category,
                PriorityScore = t.PriorityScore,
                Status = t.Status,
                CourseCode = t.Syllabus.Course.Code,
                CourseTitle = t.Syllabus.Course.Title,
                UpdatedDate = t.UpdatedDate,
            })
            .ToListAsync(cancellationToken);
    }
}
