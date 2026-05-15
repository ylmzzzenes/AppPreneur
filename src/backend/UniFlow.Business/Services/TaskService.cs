using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Tasks;
using UniFlow.DataAccess.Queries;
using UniFlow.DataAccess.UnitOfWork;
using UniFlow.Entity.Entities;
using UniFlow.Entity.ReadModels;
using UniFlow.Entity.Results;

namespace UniFlow.Business.Services;

public sealed class TaskService(ITaskQueries taskQueries, IUnitOfWork unitOfWork) : ITaskService
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

        return Result<TaskItemResponse>.Success(Map(entity));
    }

    public async Task<Result<TaskItemResponse>> UpdateStatusAsync(
        long userId,
        long taskId,
        TaskStatusUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await taskQueries.GetOwnedForUpdateAsync(taskId, userId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return Result<TaskItemResponse>.Fail("TASK_NOT_FOUND", "Task was not found.");
        }

        entity.Status = request.Status;
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<TaskItemResponse>.Success(Map(entity));
    }

    private static TaskItemResponse Map(TaskItemSummary s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Description = s.Description,
        DueDate = s.DueDate,
        Category = s.Category,
        PriorityScore = s.PriorityScore,
        Status = s.Status,
        CourseId = s.CourseId,
        CourseCode = s.CourseCode,
        CourseTitle = s.CourseTitle,
        SyllabusId = s.SyllabusId,
        SyllabusTitle = s.SyllabusTitle,
    };

    private static TaskItemResponse Map(TaskItem entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Description = entity.Description,
        DueDate = entity.DueDate,
        Category = entity.Category,
        PriorityScore = entity.PriorityScore,
        Status = entity.Status,
        CourseId = entity.Syllabus.CourseId,
        CourseCode = entity.Syllabus.Course.Code,
        CourseTitle = entity.Syllabus.Course.Title,
        SyllabusId = entity.SyllabusId,
        SyllabusTitle = entity.Syllabus.Title,
    };
}
