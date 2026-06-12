using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class ChatViewModel(IApiClient apiClient, IAuthTokenStore tokenStore) : ObservableObject
{
    public ObservableCollection<ChatMessageModel> Messages { get; } = new();

    [ObservableProperty]
    private string draftMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private bool isThinking;

    [ObservableProperty]
    private string? statusMessage;

    partial void OnDraftMessageChanged(string value)
    {
        SendCommand.NotifyCanExecuteChanged();
    }

    partial void OnStatusMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(ShowStatusBanner));
    }

    public bool ShowStatusBanner => !string.IsNullOrWhiteSpace(StatusMessage);

    private bool CanSend => !IsThinking && !string.IsNullOrWhiteSpace(DraftMessage);

    [RelayCommand(CanExecute = nameof(CanSend))]
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

            if (await AuthSessionNavigator.HandleUnauthorizedIfNeededAsync(
                    result.Error?.Code, tokenStore, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Data))
                {
                    StatusMessage = result.Error?.Message ?? "Yanıt alınamadı.";
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
