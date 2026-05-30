using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class UpdateOnboardingRequestDto
{
    public string? DisplayName { get; set; }

    public string? Major { get; set; }

    public string? AcademicGoal { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PersonalityVibeDto? PersonalityVibe { get; set; }

    public int? DailyStudyTargetMinutes { get; set; }
}
