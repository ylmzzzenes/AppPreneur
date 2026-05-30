using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Models;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class SyllabusPreviewItemViewModel(SyllabusDetectedItemDto item) : ObservableObject
{
    public SyllabusDetectedItemDto Item { get; } = item;

    [ObservableProperty]
    private bool isSelected = true;

    public string Title => Item.Title;

    public string? Type => Item.Type;

    public DateTime? DueDate => Item.DueDate;

    public int? PriorityScore => Item.PriorityScore;
}

public partial class SyllabusPreviewViewModel(IApiClient apiClient, ISyllabusScanState scanState) : ObservableObject
{
    public ObservableCollection<SyllabusPreviewItemViewModel> Items { get; } = new();

    [ObservableProperty]
    private string courseCode = string.Empty;

    [ObservableProperty]
    private string courseTitle = string.Empty;

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isBusy;

    private Guid _scanId;

    public void LoadFromState()
    {
        Items.Clear();
        var scan = scanState.Current;
        if (scan is null)
        {
            StatusMessage = "Önizleme oturumu bulunamadı. Lütfen tekrar tarayın.";
            return;
        }

        _scanId = scan.ScanId;
        CourseCode = scan.CourseCode;
        CourseTitle = scan.CourseTitle;

        foreach (var item in scan.DetectedItems)
        {
            Items.Add(new SyllabusPreviewItemViewModel(item));
        }
    }

    [RelayCommand]
    private async Task ConfirmAsync(CancellationToken cancellationToken)
    {
        var selected = Items.Where(i => i.IsSelected).Select(i => i.Item).ToList();
        if (selected.Count == 0)
        {
            StatusMessage = "En az bir görev seçmelisiniz.";
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

    [RelayCommand]
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..").ConfigureAwait(false);
}
