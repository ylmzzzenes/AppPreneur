using Microsoft.EntityFrameworkCore;
using UniFlow.DataAccess.Persistence;
using UniFlow.Entity.Constants;
using UniFlow.Entity.Entities;
using UniFlow.Entity.Enums;
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
        return await BuildSummaryQuery(userId)
            .OrderByDescending(t => t.PriorityScore)
            .ThenBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItemSummary>> ListTodayForUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await BuildSummaryQuery(userId)
            .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == today)
            .OrderByDescending(t => t.PriorityScore)
            .ThenBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItemSummary>> ListUpcomingForUserAsync(
        long userId,
        int days,
        TaskItemStatus? status,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var end = today.AddDays(days);

        var query = BuildSummaryQuery(userId)
            .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date > today && t.DueDate.Value.Date <= end);

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        return await query
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.PriorityScore)
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

    public Task<Syllabus?> GetManualSyllabusForCourseAsync(long courseId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Syllabi
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.CourseId == courseId && s.Title == ManualTaskSyllabusConstants.Title,
                cancellationToken);
    }

    public Task<Syllabus?> GetManualSyllabusForCourseForUpdateAsync(long courseId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Syllabi
            .FirstOrDefaultAsync(
                s => s.CourseId == courseId && s.Title == ManualTaskSyllabusConstants.Title,
                cancellationToken);
    }

    private IQueryable<TaskItemSummary> BuildSummaryQuery(long userId) =>
        _dbContext.TaskItems
            .AsNoTracking()
            .Where(t => t.Syllabus.Course.UserId == userId)
            .Select(t => new TaskItemSummary
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                Category = t.Category,
                PriorityScore = t.PriorityScore,
                EstimatedMinutes = t.EstimatedMinutes,
                Status = t.Status,
                CourseId = t.Syllabus.CourseId,
                CourseCode = t.Syllabus.Course.Code,
                CourseTitle = t.Syllabus.Course.Title,
                SyllabusId = t.SyllabusId,
                SyllabusTitle = t.Syllabus.Title,
                CreatedDate = t.CreatedDate,
                UpdatedDate = t.UpdatedDate,
            });
}
