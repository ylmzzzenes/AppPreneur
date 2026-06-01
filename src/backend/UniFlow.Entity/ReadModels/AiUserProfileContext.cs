using UniFlow.Entity.Enums;

namespace UniFlow.Entity.ReadModels;

public sealed class AiUserProfileContext
{
    public string DisplayName { get; init; } = string.Empty;

    public string? Major { get; init; }

    public string? AcademicGoal { get; init; }

    public PersonalityVibe PersonalityVibe { get; init; } = PersonalityVibe.Friendly;
}
