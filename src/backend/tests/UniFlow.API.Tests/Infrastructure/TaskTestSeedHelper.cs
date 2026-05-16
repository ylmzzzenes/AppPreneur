using Microsoft.Extensions.DependencyInjection;
using UniFlow.Business.Contracts.Auth;
using UniFlow.DataAccess.Persistence;
using UniFlow.Entity.Entities;
using UniFlow.Entity.Enums;

namespace UniFlow.API.Tests.Infrastructure;

internal sealed class TaskOwnershipSeed
{
    public required AuthResponse UserA { get; init; }

    public required AuthResponse UserB { get; init; }

    public long UserATaskId { get; init; }
}

internal static class TaskTestSeedHelper
{
    public static async Task<TaskOwnershipSeed> CreateTwoUsersWithTaskForUserAAsync(
        HttpClient anonymousClient,
        UniFlowApiFactory factory,
        CancellationToken cancellationToken = default)
    {
        var userARequest = AuthTestHelper.CreateValidRegisterRequest("task-owner-a");
        var userBRequest = AuthTestHelper.CreateValidRegisterRequest("task-owner-b");

        var userA = await AuthTestHelper.RegisterAsync(anonymousClient, userARequest, cancellationToken)
            .ConfigureAwait(false);
        var userB = await AuthTestHelper.RegisterAsync(anonymousClient, userBRequest, cancellationToken)
            .ConfigureAwait(false);

        if (!userA.IsSuccess || userA.Data is null)
        {
            throw new InvalidOperationException($"Failed to register user A: {userA.Error?.Code}");
        }

        if (!userB.IsSuccess || userB.Data is null)
        {
            throw new InvalidOperationException($"Failed to register user B: {userB.Error?.Code}");
        }

        var taskId = await SeedTaskForUserAsync(factory, userA.Data.UserId, cancellationToken)
            .ConfigureAwait(false);

        return new TaskOwnershipSeed
        {
            UserA = userA.Data,
            UserB = userB.Data,
            UserATaskId = taskId,
        };
    }

    private static async Task<long> SeedTaskForUserAsync(
        UniFlowApiFactory factory,
        long userId,
        CancellationToken cancellationToken)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UniFlowDbContext>();

        var course = new Course
        {
            UserId = userId,
            Code = "TST101",
            Title = "Ownership Test Course",
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var syllabus = new Syllabus
        {
            CourseId = course.Id,
            Title = "Spring 2026",
            SourceText = "seeded for integration tests",
        };
        db.Syllabi.Add(syllabus);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var task = new TaskItem
        {
            SyllabusId = syllabus.Id,
            Title = "User A Midterm",
            Category = "Exam",
            Status = TaskItemStatus.Pending,
            PriorityScore = 50,
        };
        db.TaskItems.Add(task);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return task.Id;
    }
}
