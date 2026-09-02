using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.Recovery;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Pictures;
using Eede.Application.UseCase.Settings;
using Eede.Application.UseCase.Updates;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Palettes.Persistence;
using Eede.Infrastructure.Pictures;
using Eede.Infrastructure.Recovery;
using Eede.Infrastructure.Settings;
using Eede.Infrastructure.Updates;
using Eede.Infrastructure.Launchers;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Files;
using Eede.Presentation.Launchers;
using Eede.Presentation.Services;
using Eede.Presentation.Theming;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.General;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ReactiveUI;

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
            singleViewPlatform.MainView = new MainView
            {
                DataContext = Services?.GetRequiredService<MainViewModel>()
            };
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
            if (Avalonia.Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleView && singleView.MainView != null)
            {
                var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(singleView.MainView);
                if (topLevel != null)
                {
                    return new AvaloniaFileStorage(topLevel.StorageProvider);
                }
            }
            throw new InvalidOperationException("StorageProvider is not available.");
        });
        services.AddSingleton<IDrawStyleFactory, DrawStyleFactory>();
        services.AddTransient<ITransformImageUseCase, TransformImageUseCase>();
        services.AddTransient<IScalingImageUseCase, ScalingImageUseCase>();
        services.AddTransient<IImageResampler, NearestNeighborResampler>();
        services.AddTransient<ITransferImageToCanvasUseCase, TransferImageToCanvasUseCase>();
        services.AddTransient<ITransferImageFromCanvasUseCase, TransferImageFromCanvasUseCase>();
        services.AddSingleton<IDrawingSessionProvider, DrawingSessionProvider>();
        services.AddSingleton<IFileSystem, AvaloniaFileSystem>();
        if (OperatingSystem.IsBrowser())
        {
            services.AddSingleton<IExternalBrowserLauncher, WebExternalBrowserLauncher>();
        }
        else
        {
            services.AddSingleton<IExternalBrowserLauncher, ExternalBrowserLauncher>();
        }
        services.AddSingleton<IThemeDetector, AvaloniaThemeDetector>();
        services.AddTransient<IDrawActionUseCase, DrawActionUseCase>();
        services.AddTransient<ICopySelectionUseCase, CopySelectionUseCase>();
        services.AddTransient<ICutSelectionUseCase, CutSelectionUseCase>();
        services.AddTransient<IPasteFromClipboardUseCase, PasteFromClipboardUseCase>();
        services.AddTransient<ISelectionClipboard, SelectionClipboard>();

        services.AddSingleton<IAnimationPatternsProvider, AnimationPatternsProvider>();
        services.AddTransient<IAddAnimationPatternUseCase, AddAnimationPatternUseCase>();
        services.AddTransient<IReplaceAnimationPatternUseCase, ReplaceAnimationPatternUseCase>();
        services.AddTransient<IRemoveAnimationPatternUseCase, RemoveAnimationPatternUseCase>();
        services.AddTransient<IAnimationPatternService, AnimationPatternService>();

        services.AddSingleton<IBitmapAdapter<Avalonia.Media.Imaging.Bitmap>, AvaloniaBitmapAdapter>();
        services.AddSingleton<IPictureRepository>(sp =>
            new PictureRepository(
                sp.GetRequiredService<IBitmapAdapter<Avalonia.Media.Imaging.Bitmap>>(),
                () =>
                {
                    try
                    {
                        return sp.GetService<IFileStorage>();
                    }
                    catch
                    {
                        return null;
                    }
                }
            ));
        services.AddSingleton<ISavePictureUseCase, SavePictureUseCase>();
        services.AddSingleton<ILoadPictureUseCase, LoadPictureUseCase>();
        services.AddSingleton<IPictureFileIO, PictureFileIO>();
        services.AddSingleton<IPaletteRepository, Eede.Infrastructure.Palettes.Persistence.PaletteRepository>();
        if (OperatingSystem.IsBrowser())
        {
            services.AddSingleton<IPaletteSessionRepository, LocalStoragePaletteSessionRepository>();
            services.AddSingleton<ISettingsRepository, LocalStorageSettingsRepository>();
            services.AddSingleton<IAppUpdater, NullAppUpdater>();
        }
        else
        {
            services.AddSingleton<IPaletteSessionRepository>(sp =>
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var path = System.IO.Path.Combine(appData, "Eede", "palettes_session.json");
                return new Eede.Infrastructure.Palettes.Persistence.PaletteSessionRepository(path);
            });
            services.AddSingleton<ISettingsRepository>(sp =>
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var path = System.IO.Path.Combine(appData, "Eede", "settings.json");
                return new Eede.Infrastructure.Settings.JsonSettingsRepository(path);
            });
            services.AddSingleton<IAppUpdater>(sp => new VelopackAppUpdater(@"https://github.com/arkfinn/Eede"));
        }
        services.AddTransient<ILoadSettingsUseCase, LoadSettingsUseCase>();
        services.AddTransient<ISaveSettingsUseCase, SaveSettingsUseCase>();
        services.AddTransient<CheckUpdateUseCase>();

        // Session Recovery Services
        services.AddSingleton<IPictureCodec, SkiaSharpPictureCodec>();
        services.AddSingleton<IPullContextTracker, PullContextTracker>();
        services.AddSingleton<ISessionStorage>(sp =>
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrWhiteSpace(appData))
            {
                appData = System.IO.Path.GetTempPath();
            }
            var recoveryDir = System.IO.Path.Combine(appData, "Eede", "session_recovery");
            return new LocalFileSessionStorage(recoveryDir);
        });
        services.AddSingleton<ISessionRecoverer, SessionRecoverer>();
        services.AddSingleton<SessionRecoveryCoordinator>(sp =>
            new SessionRecoveryCoordinator(
                sp.GetRequiredService<ISessionStorage>(),
                sp.GetRequiredService<IPictureCodec>(),
                () => sp.GetRequiredService<MainViewModel>().CaptureSession()
            ));

        // ViewModels
        services.AddTransient<IInteractionCoordinator, InteractionCoordinator>();
        services.AddSingleton<InjectableDockFactory>();
        services.AddTransient<PaletteContainerViewModel>();
        services.AddSingleton<AnimationViewModel>();
        services.AddSingleton<IAddFrameProvider>(sp => sp.GetRequiredService<AnimationViewModel>());
        services.AddTransient<AnimationDockViewModel>();
        services.AddTransient<DrawableCanvasViewModel>();
        services.AddTransient<DrawingSessionViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<NewPictureWindowViewModel>();
        services.AddTransient<DockPictureViewModel>();
        services.AddSingleton<WelcomeViewModel>();

        // Factories
        services.AddSingleton<Func<DockPictureViewModel>>(sp => () => sp.GetRequiredService<DockPictureViewModel>());
        services.AddSingleton<Func<NewPictureWindowViewModel>>(sp => () => sp.GetRequiredService<NewPictureWindowViewModel>());
    }
}
