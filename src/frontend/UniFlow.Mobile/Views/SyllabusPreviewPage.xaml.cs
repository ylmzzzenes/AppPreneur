using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class SyllabusPreviewPage : ContentPage
{
    public SyllabusPreviewPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<SyllabusPreviewViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SyllabusPreviewViewModel vm)
        {
            vm.LoadFromState();
        }
    }
}
