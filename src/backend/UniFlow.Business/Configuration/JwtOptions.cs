using System.ComponentModel.DataAnnotations;

namespace UniFlow.Business.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Symmetric signing key. Configure via user-secrets or environment variables (never commit).
    /// </summary>
    [Required]
    [MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 60;
}
