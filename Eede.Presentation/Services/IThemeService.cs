using Avalonia.Styling;

namespace Eede.Presentation.Services;

public interface IThemeService
{
    ThemeVariant GetActualThemeVariant();
}
