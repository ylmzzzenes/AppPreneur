using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class ChatPage : ContentPage
{
    public ChatPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<ChatViewModel>();
    }
}
