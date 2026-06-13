namespace UniFlow.Mobile;

/// <summary>
/// Resolves the backend API base URL for <see cref="Services.ApiClient"/>.
/// Priority: UNIFLOW_API_BASE_URL env var → build configuration default.
/// </summary>
internal static class ApiConstants
{
    /// <summary>Live Hetzner API (Release builds).</summary>
    public const string ProductionBaseUrl = "http://49.13.89.74/";

    /// <summary>Android Debug — temporarily points to live Hetzner API.</summary>
    public const string DevelopmentAndroidBaseUrl = "http://49.13.89.74/";

    /// <summary>Windows / iOS simulator local API (Debug).</summary>
    public const string DevelopmentDesktopBaseUrl = "http://127.0.0.1:5000/";

    /// <summary>Legacy dotnet run port when not using Docker.</summary>
    public const string DevelopmentDotnetRunBaseUrl = "http://10.0.2.2:5087/";

    public static string BaseUrl { get; } = Resolve();

    public static bool IsProductionUrl =>
        string.Equals(BaseUrl, ProductionBaseUrl, StringComparison.OrdinalIgnoreCase);

    private static string Resolve()
    {
        var raw = Environment.GetEnvironmentVariable("UNIFLOW_API_BASE_URL");
        if (!string.IsNullOrWhiteSpace(raw))
        {
            return Normalize(raw);
        }

#if DEBUG
        return DeviceInfo.Platform == DevicePlatform.Android
            ? DevelopmentAndroidBaseUrl
            : DevelopmentDesktopBaseUrl;
#else
        return ProductionBaseUrl;
#endif
    }

    private static string Normalize(string url)
    {
        var u = url.Trim();
        return u.EndsWith('/') ? u : u + "/";
    }
}
