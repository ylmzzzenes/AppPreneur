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

    Task<ApiResultDto<string>> SendChatAsync(string message, CancellationToken cancellationToken = default);
}
