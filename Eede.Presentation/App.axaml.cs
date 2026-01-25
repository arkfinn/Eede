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
using Microsoft.Extensions.DependencyInjection;
using Eede.Presentation.Common.Adapters;
using Eede.Domain.ImageEditing;
using Eede.Application.Pictures;
using System;

namespace Eede.Presentation;

public partial class App : Avalonia.Application
{
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Core/Domain/Application Services
        services.AddSingleton<GlobalState>();
        services.AddSingleton<IAnimationService, AnimationService>();
        services.AddSingleton<IClipboardService, AvaloniaClipboardService>();

        // Adapters / Infrastructure
        services.AddSingleton<IBitmapAdapter<Avalonia.Media.Imaging.Bitmap>, AvaloniaBitmapAdapter>();
        services.AddSingleton<IPictureRepository, PictureRepository>();

        // ViewModels
        services.AddTransient<MainViewModel>();
    }
}
