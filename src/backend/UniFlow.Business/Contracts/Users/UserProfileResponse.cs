using UniFlow.Entity.Enums;

namespace UniFlow.Business.Contracts.Users;

public sealed class UserProfileResponse
{
    public long UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public PersonalityVibe PersonalityVibe { get; set; }

    public string? Major { get; set; }
}
