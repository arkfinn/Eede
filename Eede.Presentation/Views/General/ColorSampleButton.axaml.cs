using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Eede.Presentation.Common;
using System;
using System.Windows.Input;

namespace Eede.Presentation.Views.General;

public partial class ColorSampleButton : UserControl
{
    private const int TOUCH_LONG_PRESS_MS = 450;
    private bool _isTouchPressing;
    private bool _touchLongPressTriggered;
    private DispatcherTimer? _touchLongPressTimer;

    public ColorSampleButton()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<Color> SampleColorProperty =
        AvaloniaProperty.Register<ColorSampleButton, Color>(nameof(SampleColor), Color.FromArgb(0, 0, 0, 0));
    public Color SampleColor
    {
        get => GetValue(SampleColorProperty);
        set => SetValue(SampleColorProperty, value);
    }

    public static readonly StyledProperty<ICommand> GetCommandProperty =
        AvaloniaProperty.Register<ColorSampleButton, ICommand>(nameof(GetCommand));
    public ICommand GetCommand
    {
        get => GetValue(GetCommandProperty);
        set => SetValue(GetCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> PutCommandProperty =
        AvaloniaProperty.Register<ColorSampleButton, ICommand>(nameof(PutCommand));
    public ICommand PutCommand
    {
        get => GetValue(PutCommandProperty);
        set => SetValue(PutCommandProperty, value);
    }

    public static readonly StyledProperty<TouchCanvasMode> TouchCanvasModeProperty =
        AvaloniaProperty.Register<ColorSampleButton, TouchCanvasMode>(nameof(TouchCanvasMode), TouchCanvasMode.Draw);
    public TouchCanvasMode TouchCanvasMode { get => GetValue(TouchCanvasModeProperty); set => SetValue(TouchCanvasModeProperty, value); }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Touch)
        {
            e.Pointer.Capture(sender as IInputElement ?? this);
            HandleTouchPressed();
            e.Handled = true;
            return;
        }
        PointerPointProperties pointer = e.GetCurrentPoint(this).Properties;
        if (pointer.IsLeftButtonPressed) PutCommand?.Execute(null);
        if (pointer.IsRightButtonPressed) GetCommand?.Execute(null);
    }

    private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (e.Pointer.Type != PointerType.Touch) return;
        e.Pointer.Capture(null);
        HandleTouchReleased();
        e.Handled = true;
    }

    private void OnPointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
    {
        _touchLongPressTimer?.Stop();
        _isTouchPressing = false;
        _touchLongPressTriggered = false;
    }

    internal void HandleTouchPressed()
    {
        _touchLongPressTimer?.Stop();
        _isTouchPressing = false;
        _touchLongPressTriggered = false;
        if (TouchCanvasMode == TouchCanvasMode.Eyedropper) { GetCommand?.Execute(null); return; }
        _isTouchPressing = true;
        _touchLongPressTimer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(TOUCH_LONG_PRESS_MS), DispatcherPriority.Input, (_, _) => TriggerTouchLongPress());
        _touchLongPressTimer.Start();
    }

    internal bool TriggerTouchLongPress()
    {
        _touchLongPressTimer?.Stop();
        if (!_isTouchPressing || _touchLongPressTriggered) return false;
        _touchLongPressTriggered = true;
        _isTouchPressing = false;
        GetCommand?.Execute(null);
        return true;
    }

    internal void HandleTouchReleased()
    {
        _touchLongPressTimer?.Stop();
        if (_isTouchPressing && !_touchLongPressTriggered) { _isTouchPressing = false; PutCommand?.Execute(null); }
        _touchLongPressTriggered = false;
    }
}
