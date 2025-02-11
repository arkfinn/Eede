using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System.Windows.Input;

namespace Eede.Presentation.Views.General;

public partial class ColorSampleButton : UserControl
{
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

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        PointerPointProperties pointer = e.GetCurrentPoint(this).Properties;
        if (pointer.IsLeftButtonPressed)
        {
            PutCommand?.Execute(null);
        }

        if (pointer.IsRightButtonPressed)
        {
            GetCommand?.Execute(null);
        }
    }
}
