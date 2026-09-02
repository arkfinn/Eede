using Avalonia.Styling;

namespace Eede.Presentation.Theming;

public interface IThemeDetector
{
    ThemeVariant GetActualThemeVariant();
}
