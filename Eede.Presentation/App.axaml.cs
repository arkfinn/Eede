using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Views.Pages;
using Eede.Application.Animations;
using System.Collections.Generic;
using Eede.Domain.Animations;
using Eede.Presentation.Services;
using Eede.Application.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Eede.Presentation.Common.Adapters;
using Eede.Domain.ImageEditing;
using Eede.Application.Pictures;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.Common.Services;
using Eede.Presentation.Files;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Application.Drawings;
using Eede.Application.UseCase.Pictures;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.DataDisplay;
using System;

namespace Eede.Presentation;

public partial class App : Avalonia.Application
{
    public static IServiceProvider? Services { get; private set; }

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
            desktop.MainWindow = new MainWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Core/Domain/Application Services
        services.AddSingleton<GlobalState>();
        services.AddSingleton<IAnimationService, AnimationService>();
        services.AddSingleton<IClipboard, AvaloniaClipboardService>();
        services.AddTransient<IFileStorage>(sp =>
        {
            var lifetime = (IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current.ApplicationLifetime;
            return new StorageService(lifetime.MainWindow.StorageProvider);
        });
        services.AddSingleton<IDrawStyleFactory, DrawStyleFactory>();
        services.AddTransient<ITransformImageUseCase, TransformImageUseCase>();
        services.AddTransient<ITransferImageToCanvasUseCase, TransferImageToCanvasUseCase>();
        services.AddTransient<ITransferImageFromCanvasUseCase, TransferImageFromCanvasUseCase>();
        services.AddSingleton<IDrawingSessionProvider, DrawingSessionProvider>();
        services.AddSingleton<IFileSystem, RealFileSystem>();
        services.AddTransient<IDrawActionUseCase, DrawActionUseCase>();
        services.AddTransient<CopySelectionUseCase>();
        services.AddTransient<CutSelectionUseCase>();
        services.AddTransient<PasteFromClipboardUseCase>();

        // Adapters / Infrastructure
        services.AddSingleton<IBitmapAdapter<Avalonia.Media.Imaging.Bitmap>, AvaloniaBitmapAdapter>();
        services.AddSingleton<IPictureRepository, PictureRepository>();
        services.AddSingleton<SavePictureUseCase>();
        services.AddSingleton<LoadPictureUseCase>();

        // ViewModels
        services.AddTransient<IInteractionCoordinator, InteractionCoordinator>();
        services.AddSingleton<InjectableDockFactory>();
        services.AddTransient<PaletteContainerViewModel>();
        services.AddSingleton<AnimationViewModel>();
        services.AddSingleton<IAddFrameProvider>(sp => sp.GetRequiredService<AnimationViewModel>());
        services.AddTransient<AnimationDockViewModel>();
        services.AddTransient<DrawableCanvasViewModel>();
        services.AddTransient<DrawingSessionViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<NewPictureWindowViewModel>();
        services.AddTransient<DockPictureViewModel>();
    }
}
