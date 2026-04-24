using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Tasks;
using UniFlow.DataAccess.Queries;
using UniFlow.Entity.ReadModels;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services;

public sealed class TaskService(ITaskQueries taskQueries) : ITaskService
{
    public async Task<Result<IReadOnlyList<TaskItemResponse>>> GetMyTasksAsync(long userId, CancellationToken cancellationToken = default)
    {
        var rows = await taskQueries.ListForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        var list = rows.Select(Map).ToList();
        return Result<IReadOnlyList<TaskItemResponse>>.Success(list);
    }

    public async Task<Result<TaskItemResponse>> GetMyTaskAsync(long userId, long taskId, CancellationToken cancellationToken = default)
    {
        var entity = await taskQueries.GetOwnedAsync(taskId, userId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return Result<TaskItemResponse>.Fail("TASK_NOT_FOUND", "Task was not found.");
        }

        return Result<TaskItemResponse>.Success(new TaskItemResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            DueDate = entity.DueDate,
            Category = entity.Category,
            PriorityScore = entity.PriorityScore,
            CourseId = entity.Syllabus.CourseId,
            CourseCode = entity.Syllabus.Course.Code,
            CourseTitle = entity.Syllabus.Course.Title,
            SyllabusId = entity.SyllabusId,
            SyllabusTitle = entity.Syllabus.Title,
        });
    }

    private static TaskItemResponse Map(TaskItemSummary s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Description = s.Description,
        DueDate = s.DueDate,
        Category = s.Category,
        PriorityScore = s.PriorityScore,
        CourseId = s.CourseId,
        CourseCode = s.CourseCode,
        CourseTitle = s.CourseTitle,
        SyllabusId = s.SyllabusId,
        SyllabusTitle = s.SyllabusTitle,
    };
}
