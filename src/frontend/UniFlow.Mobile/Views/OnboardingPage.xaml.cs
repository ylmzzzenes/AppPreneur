using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class OnboardingPage : ContentPage
{
    public OnboardingPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<OnboardingViewModel>();
    }
}
