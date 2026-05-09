using Microsoft.Maui.Controls;
using UniFlow.Mobile.ViewModels;

namespace UniFlow.Mobile.Views;

public partial class LoginPage : ContentPage
{
    private const string StrokeAnimationKey = "LoginFieldStroke";

    public LoginPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetRequiredService<LoginViewModel>();
    }

    private void OnFieldFocused(object? sender, FocusEventArgs e)
    {
        if (sender is not Entry entry || FindOutlinedBorder(entry) is not Border border)
            return;
        AnimateToFocus(border);
    }

    private void OnFieldUnfocused(object? sender, FocusEventArgs e)
    {
        if (sender is not Entry entry || FindOutlinedBorder(entry) is not Border border)
            return;
        AnimateToIdle(border);
    }

    private void AnimateToFocus(Border border)
    {
        AbortStrokeAnimation();
        var idle = IdleStrokeColor();
        var focus = FocusStrokeColor();
        var anim = new Animation(t =>
            {
                var p = Clamp01(t);
                border.Stroke = new SolidColorBrush(LerpRgb(idle, focus, p));
                border.StrokeThickness = 1 + p;
            });
        anim.Commit(this, StrokeAnimationKey, length: 220, easing: Easing.CubicOut);
    }

    private void AnimateToIdle(Border border)
    {
        AbortStrokeAnimation();
        var idle = IdleStrokeColor();
        var focus = FocusStrokeColor();
        var anim = new Animation(t =>
            {
                var p = Clamp01(t);
                border.Stroke = new SolidColorBrush(LerpRgb(focus, idle, p));
                border.StrokeThickness = 2 - p;
            });
        anim.Commit(this, StrokeAnimationKey, length: 220, easing: Easing.CubicOut);
    }

    private void AbortStrokeAnimation() => this.AbortAnimation(StrokeAnimationKey);

    private static Border? FindOutlinedBorder(Element? view)
    {
        while (view is not null && view is not Border)
            view = view.Parent as Element;

        return view as Border;
    }

    private static Color IdleStrokeColor()
    {
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        return dark ? Color.FromArgb("#475569") : Color.FromArgb("#E2E8F0");
    }

    private static Color FocusStrokeColor() => Color.FromArgb("#6366F1");

    private static double Clamp01(double v) =>
        v < 0 ? 0 : v > 1 ? 1 : v;

    private static Color LerpRgb(Color from, Color to, double progress)
    {
        var r = from.Red + (to.Red - from.Red) * progress;
        var g = from.Green + (to.Green - from.Green) * progress;
        var b = from.Blue + (to.Blue - from.Blue) * progress;
        var a = from.Alpha + (to.Alpha - from.Alpha) * progress;
        return Color.FromRgba(r, g, b, a);
    }
}
