using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Views.Pages;

namespace Eede.Presentation;

public partial class App : Avalonia.Application
{
    public static GlobalState State { get; } = new GlobalState();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(State)
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel(State)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
