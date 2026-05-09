using System.ComponentModel;
using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class ChatPage : ContentPage
{
    private ChatViewModel? _vm;
    private IDispatcherTimer? _typingTimer;
    private int _typingTick;

    public ChatPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<ChatViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is not ChatViewModel vm)
            return;

        if (!ReferenceEquals(_vm, vm))
        {
            if (_vm != null)
                _vm.PropertyChanged -= OnVmPropertyChanged;

            _vm = vm;
            _vm.PropertyChanged += OnVmPropertyChanged;
        }

        RestartTypingTickerIfNeeded(vm.IsThinking);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopTypingTicker();

        if (_vm != null)
        {
            _vm.PropertyChanged -= OnVmPropertyChanged;
            _vm = null;
        }
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ChatViewModel vm || e.PropertyName != nameof(ChatViewModel.IsThinking))
            return;

        RestartTypingTickerIfNeeded(vm.IsThinking);
    }

    private void RestartTypingTickerIfNeeded(bool thinking)
    {
        if (!thinking)
        {
            StopTypingTicker();
            TypingDot1.Opacity = 1;
            TypingDot2.Opacity = 1;
            TypingDot3.Opacity = 1;
            return;
        }

        StopTypingTicker();
        _typingTick = 0;
        TypingDot1.Opacity = 1;
        TypingDot2.Opacity = 0.35;
        TypingDot3.Opacity = 0.35;

        _typingTimer = Dispatcher.CreateTimer();
        _typingTimer.Interval = TimeSpan.FromMilliseconds(180);
        _typingTimer.Tick += OnTypingTimerTick;
        _typingTimer.Start();
    }

    private void StopTypingTicker()
    {
        if (_typingTimer is null)
            return;

        _typingTimer.Tick -= OnTypingTimerTick;
        _typingTimer.Stop();
        _typingTimer = null;
    }

    private void OnTypingTimerTick(object? sender, EventArgs e)
    {
        _typingTick = (_typingTick + 1) % 3;
        if (_typingTick == 0)
        {
            TypingDot1.Opacity = 1;
            TypingDot2.Opacity = 0.35;
            TypingDot3.Opacity = 0.35;
        }
        else if (_typingTick == 1)
        {
            TypingDot1.Opacity = 0.35;
            TypingDot2.Opacity = 1;
            TypingDot3.Opacity = 0.35;
        }
        else
        {
            TypingDot1.Opacity = 0.35;
            TypingDot2.Opacity = 0.35;
            TypingDot3.Opacity = 1;
        }
    }
}