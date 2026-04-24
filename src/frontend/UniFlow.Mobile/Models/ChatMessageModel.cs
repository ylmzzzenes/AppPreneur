namespace UniFlow.Mobile.Models;

public sealed class ChatMessageModel
{
    public string Text { get; set; } = string.Empty;

    public bool IsFromUser { get; set; }

    public string RoleLabel => IsFromUser ? "Sen" : "Sarkastik Dahi";
}
