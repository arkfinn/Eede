using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Settings;
using Eede.Presentation.Coordinators;
using Eede.Presentation.Theming;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.General;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Pictures;
using Eede.Application.UseCase.Settings;
using Eede.Domain.Animations;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Moq;
using NUnit.Framework;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using System;

namespace Eede.Presentation.Tests;

[TestFixture]
public class ThemeTests
{
    [AvaloniaTest]
    public void InitialTheme_ShouldMatchSystemSetting_Dark()
    {
        var themeDetectorMock = new Mock<IThemeDetector>();
        themeDetectorMock.Setup(x => x.GetActualThemeVariant()).Returns(Avalonia.Styling.ThemeVariant.Dark);

        var mainVM = CreateMainViewModel(themeDetectorMock.Object);
        
        Assert.That(mainVM.SelectedThemeIndex, Is.EqualTo(1), "SelectedThemeIndex should be 1 (Dark) when system theme is Dark.");
    }

    [AvaloniaTest]
    public void InitialTheme_ShouldMatchSystemSetting_Light()
    {
        var themeDetectorMock = new Mock<IThemeDetector>();
        themeDetectorMock.Setup(x => x.GetActualThemeVariant()).Returns(Avalonia.Styling.ThemeVariant.Light);

        var mainVM = CreateMainViewModel(themeDetectorMock.Object);

        Assert.That(mainVM.SelectedThemeIndex, Is.EqualTo(0), "SelectedThemeIndex should be 0 (Light) when system theme is Light.");
    }

    private MainViewModel CreateMainViewModel(IThemeDetector themeDetector)
    {
        var state = new GlobalState();
        var clipboard = new Mock<IClipboard>().Object;
        var bitmapAdapter = new Mock<IBitmapAdapter<Bitmap>>().Object;
        var pictureRepo = new Mock<IPictureRepository>().Object;
        var drawStyleFactory = new Mock<IDrawStyleFactory>().Object;
        var transformUseCase = new Mock<ITransformImageUseCase>().Object;
        var transferToCanvas = new Mock<ITransferImageToCanvasUseCase>().Object;
        var transferFromCanvas = new Mock<ITransferImageFromCanvasUseCase>().Object;
        var sessionProviderMock = new Mock<IDrawingSessionProvider>();
        sessionProviderMock.Setup(x => x.CurrentSession).Returns(new DrawingSession(Picture.CreateEmpty(new PictureSize(16, 16))));
        var sessionProvider = sessionProviderMock.Object;
        var coordinator = new Mock<IInteractionCoordinator>().Object;

        var SelectionClipboard = new SelectionClipboard(
            new CopySelectionUseCase(clipboard),
            new CutSelectionUseCase(clipboard),
            new PasteFromClipboardUseCase(clipboard, sessionProvider)
        );

        var drawableCanvasVM = new DrawableCanvasViewModel(
            state,
            new Mock<IAddFrameProvider>().Object,
            clipboard,
            bitmapAdapter,
            sessionProvider,
            SelectionClipboard,
            coordinator
        );

        var patternsProvider = new AnimationPatternsProvider();
        var animationVM = new AnimationViewModel(
            patternsProvider,
            new AnimationPatternEditor(
                new AddAnimationPatternUseCase(patternsProvider),
                new ReplaceAnimationPatternUseCase(patternsProvider),
                new RemoveAnimationPatternUseCase(patternsProvider)
            ),
            new Mock<IFileSystem>().Object,
            new AvaloniaBitmapAdapter()
        );

        var sessionVM = new DrawingSessionViewModel(sessionProvider);
        var paletteVM = new PaletteContainerViewModel(new Mock<Eede.Application.Infrastructure.IPaletteRepository>().Object, new Mock<Eede.Application.Infrastructure.IPaletteSessionRepository>().Object);
        var settingsRepo = new Mock<ISettingsRepository>();
        settingsRepo.Setup(x => x.LoadAsync()).ReturnsAsync(new AppSettings());

        var PictureFileIO = new PictureFileIO(
            new SavePictureUseCase(pictureRepo, settingsRepo.Object),
            new LoadPictureUseCase(pictureRepo, settingsRepo.Object)
        );

        var loadUseCase = new LoadSettingsUseCase(settingsRepo.Object);
        var saveUseCase = new SaveSettingsUseCase(settingsRepo.Object);

        return new MainViewModel(
            state, clipboard, bitmapAdapter, pictureRepo, drawStyleFactory,
            transformUseCase, new Mock<IScalingImageUseCase>().Object, transferToCanvas, transferFromCanvas,
            sessionProvider, drawableCanvasVM, animationVM, sessionVM,
            paletteVM, PictureFileIO, themeDetector,
            loadUseCase, saveUseCase,
            new WelcomeViewModel(settingsRepo.Object, new Mock<IExternalBrowserLauncher>().Object),
            () => new DockPictureViewModel(state, animationVM, new AvaloniaBitmapAdapter(), PictureFileIO),
            () => new NewPictureWindowViewModel()
        );
    }
}






