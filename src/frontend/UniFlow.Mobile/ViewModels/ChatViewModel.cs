using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class ChatViewModel(IApiClient apiClient) : ObservableObject
{
    public ObservableCollection<ChatMessageModel> Messages { get; } = new();

    [ObservableProperty]
    private string draftMessage = string.Empty;

    [ObservableProperty]
    private bool isThinking;

    [ObservableProperty]
    private string? statusMessage;

    [RelayCommand]
    private async Task SendAsync(CancellationToken cancellationToken)
    {
        var text = DraftMessage.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        Messages.Add(new ChatMessageModel { Text = text, IsFromUser = true });
        DraftMessage = string.Empty;
        IsThinking = true;
        StatusMessage = null;

        try
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                StatusMessage = "İnternet bağlantısı yok.";
                return;
            }

            var result = await apiClient.SendChatAsync(text, cancellationToken).ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (!result.IsSuccess || result.Data is null)
                {
                    StatusMessage = result.Error?.Message ?? "Yanıt alınamadı.";
                    Messages.Add(new ChatMessageModel
                    {
                        Text = StatusMessage ?? "Bir hata oluştu.",
                        IsFromUser = false,
                    });
                    return;
                }

                Messages.Add(new ChatMessageModel { Text = result.Data, IsFromUser = false });
            });
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsThinking = false);
        }
    }
}
