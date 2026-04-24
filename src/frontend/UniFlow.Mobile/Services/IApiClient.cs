using UniFlow.Mobile.Models;

namespace UniFlow.Mobile.Services;

public interface IApiClient
{
    Task<ApiResultDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);

    Task<ApiResultDto<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    Task<ApiResultDto<List<TaskItemResponseDto>>> GetTasksAsync(CancellationToken cancellationToken = default);

    Task<ApiResultDto<string>> SendChatAsync(string message, CancellationToken cancellationToken = default);

    Task<ApiResultDto<SyllabusIngestionResultDto>> IngestSyllabusAsync(
        string courseCode,
        string courseTitle,
        Stream fileStream,
        string? fileName,
        string? contentType,
        CancellationToken cancellationToken = default);
}
