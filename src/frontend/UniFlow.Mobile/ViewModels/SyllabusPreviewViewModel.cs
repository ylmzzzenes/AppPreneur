using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class SyllabusPreviewItemViewModel(SyllabusPreviewViewModel parent, SyllabusDetectedItemDto item)
    : ObservableObject
{
    public SyllabusDetectedItemDto Item { get; } = item;

    [ObservableProperty]
    private bool isSelected = true;

    partial void OnIsSelectedChanged(bool value) => parent.NotifySelectionChanged();

    public string Title => Item.Title;

    public string? Type => Item.Type;

    public DateTime? DueDate => Item.DueDate;

    public int? PriorityScore => Item.PriorityScore;
}

public partial class SyllabusPreviewViewModel(IApiClient apiClient, ISyllabusScanState scanState, IAuthTokenStore tokenStore)
    : ObservableObject
{
    public ObservableCollection<SyllabusPreviewItemViewModel> Items { get; } = new();

    [ObservableProperty]
    private string courseCode = string.Empty;

    [ObservableProperty]
    private string courseTitle = string.Empty;

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private bool isBusy;

    [ObservableProperty]
    private int detectedItemCount;

    [ObservableProperty]
    private int selectedItemCount;

    [ObservableProperty]
    private bool hasSession;

    private Guid _scanId;

    internal void NotifySelectionChanged()
    {
        SelectedItemCount = Items.Count(i => i.IsSelected);
    }

    public void LoadFromState()
    {
        Items.Clear();
        var scan = scanState.Current;
        if (scan is null)
        {
            HasSession = false;
            DetectedItemCount = 0;
            SelectedItemCount = 0;
            StatusMessage = "Önizleme oturumu bulunamadı. Lütfen tekrar tarayın.";
            return;
        }

        HasSession = true;
        StatusMessage = null;
        _scanId = scan.ScanId;
        CourseCode = scan.CourseCode;
        CourseTitle = scan.CourseTitle;

        foreach (var item in scan.DetectedItems)
        {
            Items.Add(new SyllabusPreviewItemViewModel(this, item));
        }

        DetectedItemCount = Items.Count;
        SelectedItemCount = Items.Count;
    }

    private bool CanConfirm => !IsBusy && HasSession && SelectedItemCount > 0;

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task ConfirmAsync(CancellationToken cancellationToken)
    {
        var selected = Items.Where(i => i.IsSelected).Select(i => i.Item).ToList();
        if (selected.Count == 0)
        {
            StatusMessage = "En az bir görev seçmelisiniz.";
            ConfirmCommand.NotifyCanExecuteChanged();
            return;
        }

        IsBusy = true;
        StatusMessage = null;
        try
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                StatusMessage = "İnternet bağlantısı yok.";
                return;
            }

            var request = new SyllabusConfirmRequestDto
            {
                ScanId = _scanId,
                CourseCode = CourseCode,
                CourseTitle = CourseTitle,
                Items = selected,
            };

            var result = await apiClient.ConfirmSyllabusAsync(request, cancellationToken).ConfigureAwait(false);

            if (await AuthSessionNavigator.HandleUnauthorizedIfNeededAsync(
                    result.Error?.Code, tokenStore, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (!result.IsSuccess || result.Data is null)
                {
                    StatusMessage = result.Error?.Message ?? "Onay başarısız.";
                    return;
                }

                scanState.Clear();
                await Shell.Current.GoToAsync($"//{Routes.MainTabs}/{Routes.Dashboard}").ConfigureAwait(false);
            });
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsBusy = false);
        }
    }

    partial void OnIsBusyChanged(bool value) => ConfirmCommand.NotifyCanExecuteChanged();

    partial void OnStatusMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(ShowErrorBanner));
    }

    public bool ShowErrorBanner => HasSession && !string.IsNullOrWhiteSpace(StatusMessage);

    partial void OnHasSessionChanged(bool value)
    {
        ConfirmCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(ShowErrorBanner));
    }

    public string SelectionSummary => $"{SelectedItemCount} / {DetectedItemCount}";

    partial void OnSelectedItemCountChanged(int value)
    {
        ConfirmCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(SelectionSummary));
    }

    partial void OnDetectedItemCountChanged(int value) => OnPropertyChanged(nameof(SelectionSummary));

    [RelayCommand]
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..").ConfigureAwait(false);
}
