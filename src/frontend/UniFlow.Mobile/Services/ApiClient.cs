using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using UniFlow.Mobile.Models;

namespace UniFlow.Mobile.Services;

public sealed class ApiClient(HttpClient http) : IApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    public Task<ApiResultDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default) =>
        PostAsync<AuthResponseDto>("api/v1/Auth/register", request, cancellationToken);

    public Task<ApiResultDto<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default) =>
        PostAsync<AuthResponseDto>("api/v1/Auth/login", request, cancellationToken);

    public async Task<ApiResultDto<DashboardTodayResponseDto>> GetDashboardTodayAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await http.GetAsync("api/v1/dashboard/today", cancellationToken).ConfigureAwait(false);
            return await ReadResultAsync<DashboardTodayResponseDto>(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return FailureFromException<DashboardTodayResponseDto>(ex);
        }
    }

    public async Task<ApiResultDto<SyllabusScanResponseDto>> ScanSyllabusAsync(
        string courseCode,
        string courseTitle,
        Stream fileStream,
        string? fileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(courseCode), "courseCode");
            form.Add(new StringContent(courseTitle), "courseTitle");

            var streamContent = new StreamContent(fileStream);
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            }

            form.Add(streamContent, "file", fileName ?? "upload.bin");

            using var response = await http.PostAsync("api/v1/Syllabus/scan", form, cancellationToken).ConfigureAwait(false);
            return await ReadResultAsync<SyllabusScanResponseDto>(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return FailureFromException<SyllabusScanResponseDto>(ex);
        }
    }

    public Task<ApiResultDto<SyllabusIngestionResultDto>> ConfirmSyllabusAsync(
        SyllabusConfirmRequestDto request,
        CancellationToken cancellationToken = default) =>
        PostAsync<SyllabusIngestionResultDto>("api/v1/Syllabus/confirm", request, cancellationToken);

    public async Task<ApiResultDto<TaskItemResponseDto>> UpdateTaskStatusAsync(
        long taskId,
        TaskItemStatus status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await http.PatchAsJsonAsync(
                    $"api/v1/Task/{taskId}/status",
                    new TaskStatusUpdateRequestDto { Status = status },
                    JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);

            return await ReadResultAsync<TaskItemResponseDto>(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return FailureFromException<TaskItemResponseDto>(ex);
        }
    }

    public Task<ApiResultDto<string>> SendChatAsync(string message, CancellationToken cancellationToken = default) =>
        PostAsync<string>("api/v1/Chat", new ChatRequestDto { Message = message }, cancellationToken);

    public async Task<ApiResultDto<UserProfileDto>> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await http.GetAsync("api/v1/users/me", cancellationToken).ConfigureAwait(false);
            return await ReadResultAsync<UserProfileDto>(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return FailureFromException<UserProfileDto>(ex);
        }
    }

    public async Task<ApiResultDto<UserProfileDto>> UpdateOnboardingAsync(
        UpdateOnboardingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await http.PatchAsJsonAsync(
                    "api/v1/users/me/onboarding",
                    request,
                    JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);

            return await ReadResultAsync<UserProfileDto>(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return FailureFromException<UserProfileDto>(ex);
        }
    }

    private async Task<ApiResultDto<T>> PostAsync<T>(string relativeUrl, object body, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await http.PostAsJsonAsync(relativeUrl, body, JsonOptions, cancellationToken).ConfigureAwait(false);
            return await ReadResultAsync<T>(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return FailureFromException<T>(ex);
        }
    }

    private static ApiResultDto<T> FailureFromException<T>(Exception ex)
    {
        var message = ex switch
        {
            HttpRequestException => "Sunucuya bağlanılamıyor. API'nin çalıştığından ve adresin doğru olduğundan emin olun.",
            TaskCanceledException tc => MessageForTaskCanceled(tc),
            JsonException => "Sunucu yanıtı beklenen formatta değil.",
            _ => "Beklenmeyen bir hata oluştu.",
        };

        return new ApiResultDto<T>
        {
            IsSuccess = false,
            Error = new ApiErrorDto { Code = "CLIENT", Message = message },
        };
    }

    private static string MessageForTaskCanceled(TaskCanceledException ex)
    {
        if (ContainsTimeoutException(ex) || ex.Message.Contains("HttpClient.Timeout", StringComparison.OrdinalIgnoreCase))
        {
            return "Sunucu süre içinde yanıt vermedi. API'yi çalıştırdığınızdan emin olun.";
        }

        return "İstek iptal edildi.";
    }

    private static bool ContainsTimeoutException(Exception? ex)
    {
        for (var e = ex; e != null; e = e.InnerException)
        {
            if (e is TimeoutException)
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<ApiResultDto<T>> ReadResultAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var statusError = MapHttpStatusError<T>(response.StatusCode);
        if (statusError is not null)
        {
            return statusError;
        }

        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var result = await JsonSerializer.DeserializeAsync<ApiResultDto<T>>(stream, JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (result is not null)
            {
                return result;
            }

            return new ApiResultDto<T>
            {
                IsSuccess = false,
                Error = new ApiErrorDto { Code = "PARSE", Message = "Sunucu yanıtı okunamadı." },
            };
        }
        catch (Exception ex)
        {
            return FailureFromException<T>(ex);
        }
    }

    private static ApiResultDto<T>? MapHttpStatusError<T>(HttpStatusCode statusCode)
    {
        if (statusCode == HttpStatusCode.Unauthorized)
        {
            return new ApiResultDto<T>
            {
                IsSuccess = false,
                Error = new ApiErrorDto { Code = "UNAUTHORIZED", Message = "Oturum süresi doldu. Lütfen tekrar giriş yapın." },
            };
        }

        if (statusCode == HttpStatusCode.RequestEntityTooLarge)
        {
            return new ApiResultDto<T>
            {
                IsSuccess = false,
                Error = new ApiErrorDto { Code = "FILE_TOO_LARGE", Message = "Dosya boyutu çok büyük. Daha küçük bir dosya deneyin." },
            };
        }

        if (statusCode == HttpStatusCode.TooManyRequests)
        {
            return new ApiResultDto<T>
            {
                IsSuccess = false,
                Error = new ApiErrorDto { Code = "RATE_LIMIT", Message = "Çok fazla istek. Kısa süre sonra tekrar deneyin." },
            };
        }

        if ((int)statusCode >= 500)
        {
            return new ApiResultDto<T>
            {
                IsSuccess = false,
                Error = new ApiErrorDto { Code = "SERVER_ERROR", Message = "Sunucu hatası. Lütfen daha sonra tekrar deneyin." },
            };
        }

        return null;
    }
}
