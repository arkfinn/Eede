using Avalonia;
using Avalonia.Styling;

namespace Eede.Presentation.Services;

public class AvaloniaThemeService : IThemeService
{
    public ThemeVariant GetActualThemeVariant()
    {
        return Avalonia.Application.Current?.ActualThemeVariant ?? ThemeVariant.Light;
    }
}
