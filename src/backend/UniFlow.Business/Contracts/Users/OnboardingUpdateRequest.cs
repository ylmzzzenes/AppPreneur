using UniFlow.Entity.Enums;

namespace UniFlow.Business.Contracts.Users;

public sealed class OnboardingUpdateRequest
{
    public PersonalityVibe? PersonalityVibe { get; set; }

    public string? Major { get; set; }
}
