using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Pictures;
using Eede.Application.UseCase.Settings;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Infrastructure.Settings;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Files;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Eede.Presentation;

#nullable enable

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
        services.AddSingleton<IClipboard, AvaloniaClipboard>();
        services.AddTransient<IFileStorage>(sp =>
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                return new AvaloniaFileStorage(desktop.MainWindow.StorageProvider);
            }
            throw new InvalidOperationException("ClassicDesktopStyleApplicationLifetime is not available or MainWindow is null.");
        });
        services.AddSingleton<IDrawStyleFactory, DrawStyleFactory>();
        services.AddTransient<ITransformImageUseCase, TransformImageUseCase>();
        services.AddTransient<IScalingImageUseCase, ScalingImageUseCase>();
        services.AddTransient<IImageResampler, NearestNeighborResampler>();
        services.AddTransient<ITransferImageToCanvasUseCase, TransferImageToCanvasUseCase>();
        services.AddTransient<ITransferImageFromCanvasUseCase, TransferImageFromCanvasUseCase>();
        services.AddSingleton<IDrawingSessionProvider, DrawingSessionProvider>();
        services.AddSingleton<IFileSystem, AvaloniaFileSystem>();
        services.AddSingleton<IThemeService, AvaloniaThemeService>();
        services.AddTransient<IDrawActionUseCase, DrawActionUseCase>();
        services.AddTransient<ICopySelectionUseCase, CopySelectionUseCase>();
        services.AddTransient<ICutSelectionUseCase, CutSelectionUseCase>();
        services.AddTransient<IPasteFromClipboardUseCase, PasteFromClipboardUseCase>();
        services.AddTransient<ISelectionService, SelectionService>();

        services.AddSingleton<IAnimationPatternsProvider, AnimationPatternsProvider>();
        services.AddTransient<IAddAnimationPatternUseCase, AddAnimationPatternUseCase>();
        services.AddTransient<IReplaceAnimationPatternUseCase, ReplaceAnimationPatternUseCase>();
        services.AddTransient<IRemoveAnimationPatternUseCase, RemoveAnimationPatternUseCase>();
        services.AddTransient<IAnimationPatternService, AnimationPatternService>();

        // Adapters / Infrastructure
        services.AddSingleton<IBitmapAdapter<Avalonia.Media.Imaging.Bitmap>, AvaloniaBitmapAdapter>();
        services.AddSingleton<IPictureRepository, PictureRepository>();
        services.AddSingleton<ISavePictureUseCase, SavePictureUseCase>();
        services.AddSingleton<ILoadPictureUseCase, LoadPictureUseCase>();
        services.AddSingleton<IPictureIOService, PictureIOService>();

        // Settings
        services.AddSingleton<ISettingsRepository>(sp =>
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = System.IO.Path.Combine(appData, "Eede", "settings.json");
            return new Eede.Infrastructure.Settings.JsonSettingsRepository(path);
        });
        services.AddTransient<ILoadSettingsUseCase, LoadSettingsUseCase>();
        services.AddTransient<ISaveSettingsUseCase, SaveSettingsUseCase>();

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

        // Factories
        services.AddSingleton<Func<DockPictureViewModel>>(sp => () => sp.GetRequiredService<DockPictureViewModel>());
        services.AddSingleton<Func<NewPictureWindowViewModel>>(sp => () => sp.GetRequiredService<NewPictureWindowViewModel>());
    }
}
