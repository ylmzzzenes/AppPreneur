using System.Text.Json.Serialization;

namespace UniFlow.Mobile.Models;

public sealed class UserProfileDto
{
    public long Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Major { get; set; }

    public string? AcademicGoal { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PersonalityVibeDto PersonalityVibe { get; set; }

    public int? DailyStudyTargetMinutes { get; set; }

    public bool IsOnboardingCompleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
