using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Settings;
using Eede.Presentation.Services;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.Animations;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Pictures;
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
        var themeServiceMock = new Mock<IThemeService>();
        themeServiceMock.Setup(x => x.GetActualThemeVariant()).Returns(Avalonia.Styling.ThemeVariant.Dark);

        var mainVM = CreateMainViewModel(themeServiceMock.Object);
        
        Assert.That(mainVM.SelectedThemeIndex, Is.EqualTo(1), "SelectedThemeIndex should be 1 (Dark) when system theme is Dark.");
    }

    [AvaloniaTest]
    public void InitialTheme_ShouldMatchSystemSetting_Light()
    {
        var themeServiceMock = new Mock<IThemeService>();
        themeServiceMock.Setup(x => x.GetActualThemeVariant()).Returns(Avalonia.Styling.ThemeVariant.Light);

        var mainVM = CreateMainViewModel(themeServiceMock.Object);

        Assert.That(mainVM.SelectedThemeIndex, Is.EqualTo(0), "SelectedThemeIndex should be 0 (Light) when system theme is Light.");
    }

    private MainViewModel CreateMainViewModel(IThemeService themeService)
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

        var selectionService = new SelectionService(
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
            selectionService,
            coordinator
        );

        var patternsProvider = new AnimationPatternsProvider();
        var animationVM = new AnimationViewModel(
            patternsProvider,
            new AnimationPatternService(
                new AddAnimationPatternUseCase(patternsProvider),
                new ReplaceAnimationPatternUseCase(patternsProvider),
                new RemoveAnimationPatternUseCase(patternsProvider)
            ),
            new Mock<IFileSystem>().Object,
            new AvaloniaBitmapAdapter()
        );

        var sessionVM = new DrawingSessionViewModel(sessionProvider);
        var paletteVM = new PaletteContainerViewModel();
        var pictureIOService = new PictureIOService(
            new SavePictureUseCase(pictureRepo),
            new LoadPictureUseCase(pictureRepo)
        );

        return new MainViewModel(
            state, clipboard, bitmapAdapter, pictureRepo, drawStyleFactory,
            transformUseCase, new Mock<IScalingImageUseCase>().Object, transferToCanvas, transferFromCanvas,
            sessionProvider, drawableCanvasVM, animationVM, sessionVM,
            paletteVM, pictureIOService, themeService,
            () => new DockPictureViewModel(state, animationVM, new AvaloniaBitmapAdapter(), pictureIOService),
            () => new NewPictureWindowViewModel()
        );
    }
}
