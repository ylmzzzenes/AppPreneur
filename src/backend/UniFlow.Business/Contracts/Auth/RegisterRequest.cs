using UniFlow.Entity.Enums;

namespace UniFlow.Business.Contracts.Auth;

public sealed class RegisterRequest
{
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// AI tone preference. Defaults to <see cref="PersonalityVibe.Friendly"/> when omitted.
    /// </summary>
    public PersonalityVibe? PersonalityVibe { get; set; }

    /// <summary>
    /// Optional field of study (max 100 characters).
    /// </summary>
    public string? Major { get; set; }
}
