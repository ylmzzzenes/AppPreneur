using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniFlow.Mobile.Services;

namespace UniFlow.Mobile.ViewModels;

public partial class SyllabusViewModel(IApiClient apiClient) : ObservableObject
{
    [ObservableProperty]
    private string courseCode = string.Empty;

    [ObservableProperty]
    private string courseTitle = string.Empty;

    [ObservableProperty]
    private string? pickedFileLabel;

    partial void OnPickedFileLabelChanged(string? value)
    {
        OnPropertyChanged(nameof(HasPickedFile));
    }

    public bool HasPickedFile => !string.IsNullOrWhiteSpace(PickedFileLabel);

    [ObservableProperty]
    private string? statusMessage;

    partial void OnStatusMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(ShowStatusBanner));
    }

    public bool ShowStatusBanner => !string.IsNullOrWhiteSpace(StatusMessage);

    [ObservableProperty]
    private bool isBusy;

    private FileResult? _pickedFile;

    [RelayCommand]
    private async Task ChooseFileSourceAsync(CancellationToken cancellationToken)
    {
        var page = Shell.Current?.CurrentPage;
        if (page is null)
            return;

        var pick = await page.DisplayActionSheet(
            "Dosya ekle",
            "İptal",
            null,
            "Galeriden seç",
            "Kamerayla çek");

        if (pick == "Galeriden seç")
            await PickPhotoAsync(cancellationToken).ConfigureAwait(false);
        else if (pick == "Kamerayla çek")
            await CapturePhotoAsync(cancellationToken).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task PickPhotoAsync(CancellationToken cancellationToken)
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Müfredat seç" })
                .ConfigureAwait(false);
            if (photo is null)
            {
                return;
            }

            _pickedFile = photo;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                PickedFileLabel = photo.FileName;
                StatusMessage = null;
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() => StatusMessage = ex.Message);
        }
    }

    [RelayCommand]
    private async Task CapturePhotoAsync(CancellationToken cancellationToken)
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>().ConfigureAwait(false);
            if (status != PermissionStatus.Granted)
            {
                await MainThread.InvokeOnMainThreadAsync(() => StatusMessage = "Kamera izni gerekli.");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions { Title = "Müfredat çek" })
                .ConfigureAwait(false);
            if (photo is null)
            {
                return;
            }

            _pickedFile = photo;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                PickedFileLabel = photo.FileName;
                StatusMessage = null;
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() => StatusMessage = ex.Message);
        }
    }

    [RelayCommand]
    private async Task UploadAsync(CancellationToken cancellationToken)
    {
        if (_pickedFile is null)
        {
            StatusMessage = "Önce fotoğraf seçin veya çekin.";
            return;
        }

        if (string.IsNullOrWhiteSpace(CourseCode) || string.IsNullOrWhiteSpace(CourseTitle))
        {
            StatusMessage = "Ders kodu ve adı gerekli.";
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

            await using var stream = await _pickedFile.OpenReadAsync().ConfigureAwait(false);
            var result = await apiClient.IngestSyllabusAsync(
                    CourseCode.Trim(),
                    CourseTitle.Trim(),
                    stream,
                    _pickedFile.FileName,
                    null,
                    cancellationToken)
                .ConfigureAwait(false);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (!result.IsSuccess || result.Data is null)
                {
                    StatusMessage = result.Error?.Message ?? "Yükleme başarısız.";
                    return;
                }

                StatusMessage = $"Tamam: {result.Data.TaskCount} görev çıkarıldı.";
            });
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsBusy = false);
        }
    }
}
