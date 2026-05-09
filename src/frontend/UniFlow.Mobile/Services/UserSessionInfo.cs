namespace UniFlow.Mobile.Services;

public sealed class UserSessionInfo : IUserSessionInfo
{
    private const string DisplayNameKey = "uniflow_display_name";

    public void SetDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            Preferences.Remove(DisplayNameKey);
        else
            Preferences.Set(DisplayNameKey, displayName.Trim());
    }

    public void Clear() => Preferences.Remove(DisplayNameKey);

    public string GetAvatarLetter()
    {
        var name = Preferences.Get(DisplayNameKey, string.Empty);
        if (string.IsNullOrWhiteSpace(name))
            return "?";
        return char.ToUpperInvariant(name.Trim()[0]).ToString();
    }
}
