using Microsoft.EntityFrameworkCore;
using UniFlow.DataAccess.Persistence;
using UniFlow.Entity.Entities;
using UniFlow.Entity.ReadModels;

namespace UniFlow.DataAccess.Queries;

public sealed class TaskQueries : ITaskQueries
{
    private readonly UniFlowDbContext _dbContext;

    public TaskQueries(UniFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TaskItemSummary>> ListForUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaskItems
            .AsNoTracking()
            .Include(t => t.Syllabus)
            .ThenInclude(s => s.Course)
            .Where(t => t.Syllabus.Course.UserId == userId)
            .OrderByDescending(t => t.PriorityScore)
            .ThenBy(t => t.DueDate)
            .Select(t => new TaskItemSummary
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                Category = t.Category,
                PriorityScore = t.PriorityScore,
                Status = t.Status,
                CourseId = t.Syllabus.CourseId,
                CourseCode = t.Syllabus.Course.Code,
                CourseTitle = t.Syllabus.Course.Title,
                SyllabusId = t.SyllabusId,
                SyllabusTitle = t.Syllabus.Title,
            })
            .ToListAsync(cancellationToken);
    }

    public Task<TaskItem?> GetOwnedAsync(long taskId, long userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.TaskItems
            .AsNoTracking()
            .Include(t => t.Syllabus)
            .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(
                t => t.Id == taskId && t.Syllabus.Course.UserId == userId,
                cancellationToken);
    }

    public Task<TaskItem?> GetOwnedForUpdateAsync(long taskId, long userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.TaskItems
            .Include(t => t.Syllabus)
            .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(
                t => t.Id == taskId && t.Syllabus.Course.UserId == userId,
                cancellationToken);
    }
}
