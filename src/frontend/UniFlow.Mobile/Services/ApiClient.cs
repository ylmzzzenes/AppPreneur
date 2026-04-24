using System.Net.Http.Json;
using System.Text.Json;
using UniFlow.Mobile.Models;

namespace UniFlow.Mobile.Services;

public sealed class ApiClient(HttpClient http) : IApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public Task<ApiResultDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default) =>
        PostAsync<AuthResponseDto>("api/v1/Auth/register", request, cancellationToken);

    public Task<ApiResultDto<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default) =>
        PostAsync<AuthResponseDto>("api/v1/Auth/login", request, cancellationToken);

    public async Task<ApiResultDto<List<TaskItemResponseDto>>> GetTasksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await http.GetAsync("api/v1/Task", cancellationToken).ConfigureAwait(false);
            return await ReadResultAsync<List<TaskItemResponseDto>>(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return FailureFromException<List<TaskItemResponseDto>>(ex);
        }
    }

    public Task<ApiResultDto<string>> SendChatAsync(string message, CancellationToken cancellationToken = default) =>
        PostAsync<string>("api/v1/Chat", new ChatRequestDto { Message = message }, cancellationToken);

    public async Task<ApiResultDto<SyllabusIngestionResultDto>> IngestSyllabusAsync(
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
            if (!string.IsNullOrEmpty(contentType))
            {
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            }

            form.Add(streamContent, "file", fileName ?? "upload.bin");

            using var response = await http.PostAsync("api/v1/Syllabus/ingest", form, cancellationToken).ConfigureAwait(false);
            return await ReadResultAsync<SyllabusIngestionResultDto>(response, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return FailureFromException<SyllabusIngestionResultDto>(ex);
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
            _ => ex.Message,
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
            return "Sunucu süre içinde yanıt vermedi. API'yi (http profili veya http-all-interfaces) çalıştırdığınızdan emin olun; "
                + "fiziksel telefonda bilgisayarın yerel IP adresini ve UNIFLOW_API_BASE_URL kullanın.";
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
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var result = await JsonSerializer.DeserializeAsync<ApiResultDto<T>>(stream, JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            return result ?? new ApiResultDto<T> { IsSuccess = false, Error = new ApiErrorDto { Code = "PARSE", Message = "Sunucu yanıtı okunamadı." } };
        }
        catch (Exception ex)
        {
            return FailureFromException<T>(ex);
        }
    }
}
