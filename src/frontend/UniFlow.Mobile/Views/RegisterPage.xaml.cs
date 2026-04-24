using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<RegisterViewModel>();
    }
}
