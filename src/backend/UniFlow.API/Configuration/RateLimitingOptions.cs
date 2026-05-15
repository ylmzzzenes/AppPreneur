namespace UniFlow.API.Configuration;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public RateLimitPolicyOptions Ai { get; set; } = new() { PermitLimit = 10, WindowSeconds = 60 };

    public RateLimitPolicyOptions Upload { get; set; } = new() { PermitLimit = 5, WindowSeconds = 60 };
}

public sealed class RateLimitPolicyOptions
{
    public int PermitLimit { get; set; } = 10;

    public int WindowSeconds { get; set; } = 60;
}
