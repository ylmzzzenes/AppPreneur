using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class ChatRequestDto
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
