using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class SyllabusPage : ContentPage
{
    public SyllabusPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<SyllabusViewModel>();
    }
}
