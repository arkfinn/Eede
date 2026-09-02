using Avalonia;
using Avalonia.Styling;

namespace Eede.Presentation.Theming;

public class AvaloniaThemeDetector : IThemeDetector
{
    public ThemeVariant GetActualThemeVariant()
    {
        return Avalonia.Application.Current?.ActualThemeVariant ?? ThemeVariant.Light;
    }
}
