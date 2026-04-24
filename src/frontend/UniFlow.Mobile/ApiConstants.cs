namespace UniFlow.Mobile;

/// <summary>
/// Development API base URL.
/// Emulator: Android uses 10.0.2.2 → host loopback. Physical device: set UNIFLOW_API_BASE_URL
/// (e.g. http://192.168.1.10:5087/) and run API on all interfaces (see UniFlow.API launch profile http-all-interfaces).
/// </summary>
internal static class ApiConstants
{
    public static string BaseUrl { get; } = Resolve();

    private static string Resolve()
    {
        var raw = Environment.GetEnvironmentVariable("UNIFLOW_API_BASE_URL");
        if (!string.IsNullOrWhiteSpace(raw))
        {
            var u = raw.Trim();
            return u.EndsWith('/') ? u : u + "/";
        }

        return DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5087/"
            : "http://127.0.0.1:5087/";
    }
}
