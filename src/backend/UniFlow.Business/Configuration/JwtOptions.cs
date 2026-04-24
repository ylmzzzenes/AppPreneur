namespace UniFlow.Business.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "UniFlow";

    public string Audience { get; set; } = "UniFlow.Mobile";

    /// <summary>
    /// Symmetric signing key (use a long random secret in production).
    /// </summary>
    public string Key { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 60;
}
