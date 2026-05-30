using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PersonalityVibeDto
{
    Friendly,
    Strict,
    Sarcastic,
    Motivational,
    Calm,
}
