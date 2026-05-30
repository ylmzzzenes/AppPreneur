using UniFlow.Mobile.Models;

namespace UniFlow.Mobile.Services;

public interface IApiClient
{
    Task<ApiResultDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);

    Task<ApiResultDto<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    Task<ApiResultDto<DashboardTodayResponseDto>> GetDashboardTodayAsync(CancellationToken cancellationToken = default);

    Task<ApiResultDto<SyllabusScanResponseDto>> ScanSyllabusAsync(
        string courseCode,
        string courseTitle,
        Stream fileStream,
        string? fileName,
        string? contentType,
        CancellationToken cancellationToken = default);

    Task<ApiResultDto<SyllabusIngestionResultDto>> ConfirmSyllabusAsync(
        SyllabusConfirmRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResultDto<TaskItemResponseDto>> UpdateTaskStatusAsync(
        long taskId,
        TaskItemStatus status,
        CancellationToken cancellationToken = default);

    Task<ApiResultDto<UserProfileDto>> GetMyProfileAsync(CancellationToken cancellationToken = default);

    Task<ApiResultDto<UserProfileDto>> UpdateOnboardingAsync(
        UpdateOnboardingRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResultDto<List<CourseResponseDto>>> GetCoursesAsync(CancellationToken cancellationToken = default);

    Task<ApiResultDto<CourseResponseDto>> GetCourseAsync(long id, CancellationToken cancellationToken = default);

    Task<ApiResultDto<CourseResponseDto>> CreateCourseAsync(
        CreateCourseRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResultDto<CourseResponseDto>> UpdateCourseAsync(
        long id,
        UpdateCourseRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResultDto<bool>> DeleteCourseAsync(long id, CancellationToken cancellationToken = default);

    Task<ApiResultDto<List<TaskItemResponseDto>>> GetTasksAsync(CancellationToken cancellationToken = default);

    Task<ApiResultDto<TaskItemResponseDto>> GetTaskAsync(long id, CancellationToken cancellationToken = default);

    Task<ApiResultDto<TaskListResponseDto>> GetTodayTasksAsync(CancellationToken cancellationToken = default);

    Task<ApiResultDto<List<TaskItemResponseDto>>> GetUpcomingTasksAsync(
        int days = 7,
        TaskItemStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<ApiResultDto<TaskItemResponseDto>> CreateTaskAsync(
        CreateTaskRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResultDto<TaskItemResponseDto>> UpdateTaskAsync(
        long id,
        UpdateTaskRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResultDto<bool>> DeleteTaskAsync(long id, CancellationToken cancellationToken = default);

    Task<ApiResultDto<string>> SendChatAsync(string message, CancellationToken cancellationToken = default);
}
