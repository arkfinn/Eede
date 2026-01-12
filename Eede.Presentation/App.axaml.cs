using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Views.Pages;
using Eede.Application.Animations;
using System.Collections.Generic;
using Eede.Domain.Animations;

namespace Eede.Presentation;

public partial class App : Avalonia.Application
{
    public static GlobalState State { get; } = new GlobalState();
    private static readonly IAnimationService AnimationService = new AnimationService();

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
                DataContext = new MainViewModel(State, AnimationService)
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel(State, AnimationService)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
