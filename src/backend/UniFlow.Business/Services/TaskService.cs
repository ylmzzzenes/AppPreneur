using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Tasks;
using UniFlow.Business.Helpers;
using UniFlow.DataAccess.Queries;
using UniFlow.DataAccess.UnitOfWork;
using UniFlow.Entity.Constants;
using UniFlow.Entity.Entities;
using UniFlow.Entity.Enums;
using UniFlow.Entity.ReadModels;
using UniFlow.Entity.Results;
using SyllabusEntity = UniFlow.Entity.Entities.Syllabus;

namespace UniFlow.Business.Services;

public sealed class TaskService(
    ITaskQueries taskQueries,
    ICourseQueries courseQueries,
    IUnitOfWork unitOfWork) : ITaskService
{
    public async Task<Result<IReadOnlyList<TaskItemResponse>>> GetMyTasksAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var rows = await taskQueries.ListForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        return Result<IReadOnlyList<TaskItemResponse>>.Success(rows.Select(Map).ToList());
    }

    public async Task<Result<TaskListResponse>> GetTodayTasksAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var rows = await taskQueries.ListTodayForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        var items = rows.Select(Map).ToList();
        return Result<TaskListResponse>.Success(new TaskListResponse
        {
            Items = items,
            PendingCount = items.Count(i => i.Status == TaskItemStatus.Pending),
            DoneCount = items.Count(i => i.Status == TaskItemStatus.Done),
        });
    }

    public async Task<Result<IReadOnlyList<TaskItemResponse>>> GetUpcomingTasksAsync(
        long userId,
        int days,
        TaskItemStatus? status,
        CancellationToken cancellationToken = default)
    {
        var boundedDays = days is < 1 or > 30 ? 7 : days;
        var rows = await taskQueries.ListUpcomingForUserAsync(userId, boundedDays, status, cancellationToken)
            .ConfigureAwait(false);
        return Result<IReadOnlyList<TaskItemResponse>>.Success(rows.Select(Map).ToList());
    }

    public async Task<Result<TaskItemResponse>> GetMyTaskAsync(
        long userId,
        long taskId,
        CancellationToken cancellationToken = default)
    {
        var entity = await taskQueries.GetOwnedAsync(taskId, userId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return Result<TaskItemResponse>.Fail("TASK_NOT_FOUND", "Task was not found.");
        }

        return Result<TaskItemResponse>.Success(Map(entity));
    }

    public async Task<Result<TaskItemResponse>> CreateAsync(
        long userId,
        CreateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var course = await courseQueries.GetOwnedAsync(request.CourseId, userId, cancellationToken)
            .ConfigureAwait(false);
        if (course is null)
        {
            return Result<TaskItemResponse>.Fail("COURSE_NOT_FOUND", "Course was not found.");
        }

        var syllabus = await GetOrCreateManualSyllabusAsync(request.CourseId, cancellationToken).ConfigureAwait(false);
        var task = new TaskItem
        {
            SyllabusId = syllabus.Id,
            Title = request.Title.Trim(),
            Description = CourseNormalizer.NormalizeOptional(request.Description),
            DueDate = request.DueDate,
            EstimatedMinutes = request.EstimatedMinutes,
            PriorityScore = request.PriorityScore,
            Status = request.Status ?? TaskItemStatus.Pending,
        };

        unitOfWork.Repository<TaskItem>().Add(task);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await taskQueries.GetOwnedAsync(task.Id, userId, cancellationToken).ConfigureAwait(false);
        return Result<TaskItemResponse>.Success(Map(created!));
    }

    public async Task<Result<TaskItemResponse>> UpdateAsync(
        long userId,
        long taskId,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await taskQueries.GetOwnedForUpdateAsync(taskId, userId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return Result<TaskItemResponse>.Fail("TASK_NOT_FOUND", "Task was not found.");
        }

        var course = await courseQueries.GetOwnedAsync(request.CourseId, userId, cancellationToken)
            .ConfigureAwait(false);
        if (course is null)
        {
            return Result<TaskItemResponse>.Fail("COURSE_NOT_FOUND", "Course was not found.");
        }

        if (entity.Syllabus.CourseId != request.CourseId)
        {
            var syllabus = await GetOrCreateManualSyllabusAsync(request.CourseId, cancellationToken).ConfigureAwait(false);
            entity.SyllabusId = syllabus.Id;
        }

        entity.Title = request.Title.Trim();
        entity.Description = CourseNormalizer.NormalizeOptional(request.Description);
        entity.DueDate = request.DueDate;
        entity.EstimatedMinutes = request.EstimatedMinutes;
        entity.PriorityScore = request.PriorityScore;
        entity.Status = request.Status;

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await taskQueries.GetOwnedAsync(taskId, userId, cancellationToken).ConfigureAwait(false);
        return Result<TaskItemResponse>.Success(Map(updated!));
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

        var updated = await taskQueries.GetOwnedAsync(taskId, userId, cancellationToken).ConfigureAwait(false);
        return Result<TaskItemResponse>.Success(Map(updated!));
    }

    public async Task<Result<bool>> DeleteAsync(long userId, long taskId, CancellationToken cancellationToken = default)
    {
        var entity = await taskQueries.GetOwnedForUpdateAsync(taskId, userId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return Result<bool>.Fail("TASK_NOT_FOUND", "Task was not found.");
        }

        unitOfWork.Repository<TaskItem>().Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<bool>.Success(true);
    }

    private async Task<SyllabusEntity> GetOrCreateManualSyllabusAsync(long courseId, CancellationToken cancellationToken)
    {
        var existing = await taskQueries.GetManualSyllabusForCourseForUpdateAsync(courseId, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            return existing;
        }

        var syllabus = new SyllabusEntity
        {
            CourseId = courseId,
            Title = ManualTaskSyllabusConstants.Title,
        };

        unitOfWork.Repository<SyllabusEntity>().Add(syllabus);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return syllabus;
    }

    private static TaskItemResponse Map(TaskItemSummary s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Description = s.Description,
        DueDate = s.DueDate,
        Category = s.Category,
        PriorityScore = s.PriorityScore,
        EstimatedMinutes = s.EstimatedMinutes,
        Status = s.Status,
        CourseId = s.CourseId,
        CourseCode = s.CourseCode,
        CourseTitle = s.CourseTitle,
        SyllabusId = s.SyllabusId,
        SyllabusTitle = s.SyllabusTitle,
        IsAiGenerated = !string.Equals(s.SyllabusTitle, ManualTaskSyllabusConstants.Title, StringComparison.Ordinal),
        CreatedAt = s.CreatedDate,
        UpdatedAt = s.UpdatedDate,
    };

    private static TaskItemResponse Map(TaskItem entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Description = entity.Description,
        DueDate = entity.DueDate,
        Category = entity.Category,
        PriorityScore = entity.PriorityScore,
        EstimatedMinutes = entity.EstimatedMinutes,
        Status = entity.Status,
        CourseId = entity.Syllabus.CourseId,
        CourseCode = entity.Syllabus.Course.Code,
        CourseTitle = entity.Syllabus.Course.Title,
        SyllabusId = entity.SyllabusId,
        SyllabusTitle = entity.Syllabus.Title,
        IsAiGenerated = !string.Equals(entity.Syllabus.Title, ManualTaskSyllabusConstants.Title, StringComparison.Ordinal),
        CreatedAt = entity.CreatedDate,
        UpdatedAt = entity.UpdatedDate,
    };
}
