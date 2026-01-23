using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Views.Pages;
using Eede.Application.Animations;
using System.Collections.Generic;
using Eede.Domain.Animations;
using Eede.Presentation.Services;
using Eede.Application.Services;

namespace Eede.Presentation;

public partial class App : Avalonia.Application
{
    public static GlobalState State { get; } = new GlobalState();
    private static readonly IAnimationService AnimationService = new AnimationService();
    private static readonly IClipboardService ClipboardService = new AvaloniaClipboardService();

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
                DataContext = new MainViewModel(State, AnimationService, ClipboardService)
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel(State, AnimationService, ClipboardService)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
