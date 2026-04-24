using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class ApiResultDto<T>
{
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("error")]
    public ApiErrorDto? Error { get; set; }
}
