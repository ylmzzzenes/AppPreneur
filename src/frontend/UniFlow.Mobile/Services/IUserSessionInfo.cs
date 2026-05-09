namespace UniFlow.Mobile.Services;

public interface IUserSessionInfo
{
    void SetDisplayName(string? displayName);

    void Clear();

    string GetAvatarLetter();
}
