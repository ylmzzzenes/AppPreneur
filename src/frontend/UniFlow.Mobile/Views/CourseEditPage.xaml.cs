using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class CourseEditPage : ContentPage
{
    public CourseEditPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<CourseEditViewModel>();
    }
}
