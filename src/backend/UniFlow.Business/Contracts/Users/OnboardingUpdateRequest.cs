using UniFlow.Entity.Enums;

namespace UniFlow.Business.Contracts.Users;

public sealed class OnboardingUpdateRequest
{
    public string? DisplayName { get; set; }

    public string? Major { get; set; }

    public string? AcademicGoal { get; set; }

    public PersonalityVibe? PersonalityVibe { get; set; }

    public int? DailyStudyTargetMinutes { get; set; }
}
