using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Pictures;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Application.UseCase.Pictures;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Settings;
using Eede.Domain.Animations;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.History;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.General;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Views.DataEntry;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace Eede.Presentation.Tests.Views.DataEntry;

[TestFixture]
public class PictureContainerTests
{
    private MainViewModel _mainViewModel;
    private DockPictureViewModel _dockViewModel;
    private Window _window;
    private PictureContainer _container;

    [SetUp]
    public void Setup()
    {
        // 1. Core Dependencies
        var bitmapAdapter = new AvaloniaBitmapAdapter();
        var globalState = new GlobalState();

        var mockDrawingSessionProvider = new Mock<IDrawingSessionProvider>();
        mockDrawingSessionProvider.Setup(x => x.CurrentSession).Returns(new DrawingSession(Picture.CreateEmpty(new PictureSize(1, 1))));
        // DrawingSessionViewModel setup (simplified)
        var drawingSessionVM = new DrawingSessionViewModel(mockDrawingSessionProvider.Object);

        var mockClipboard = new Mock<IClipboard>();
        var mockCoordinator = new Mock<IInteractionCoordinator>();
        var mockAddFrameProvider = new Mock<IAddFrameProvider>();

        var copyUseCase = new CopySelectionUseCase(mockClipboard.Object);
        var cutUseCase = new CutSelectionUseCase(mockClipboard.Object);
        var pasteUseCase = new PasteFromClipboardUseCase(mockClipboard.Object, mockDrawingSessionProvider.Object);
        var selectionService = new SelectionService(copyUseCase, cutUseCase, pasteUseCase);

        var mockPictureRepo = new Mock<IPictureRepository>();
        var mockSettingsRepoForUseCase = new Mock<ISettingsRepository>();
        mockSettingsRepoForUseCase.Setup(x => x.LoadAsync()).ReturnsAsync(new AppSettings());
        var savePictureUseCase = new SavePictureUseCase(mockPictureRepo.Object, mockSettingsRepoForUseCase.Object);
        var loadPictureUseCase = new LoadPictureUseCase(mockPictureRepo.Object, mockSettingsRepoForUseCase.Object);
        var pictureIOService = new PictureIOService(savePictureUseCase, loadPictureUseCase);

        // 2. Sub ViewModels Dependencies
        var patternsProvider = new AnimationPatternsProvider();
        var patternService = new AnimationPatternService(
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider));
        var animationVM = new AnimationViewModel(
            patternsProvider,
            patternService,
            new Mock<IFileSystem>().Object,
            new AvaloniaBitmapAdapter());

        var transformUseCase = new Mock<ITransformImageUseCase>();
        var transferToCanvasUseCase = new Mock<ITransferImageToCanvasUseCase>();
        var transferFromCanvasUseCase = new Mock<ITransferImageFromCanvasUseCase>();

        // DrawableCanvasViewModel
        var drawableCanvasVM = new DrawableCanvasViewModel(
            globalState,
            mockAddFrameProvider.Object,
            mockClipboard.Object,
            bitmapAdapter,
            mockDrawingSessionProvider.Object,
            selectionService,
            mockCoordinator.Object
        );

        var paletteVM = new PaletteContainerViewModel();
        var mockDrawStyleFactory = new Mock<IDrawStyleFactory>();
        var settingsRepo = new Mock<ISettingsRepository>().Object;
        var loadSettingsUseCase = new LoadSettingsUseCase(settingsRepo);
        var saveSettingsUseCase = new SaveSettingsUseCase(settingsRepo);

        // MainViewModel
        _mainViewModel = new MainViewModel(
            globalState,
            mockClipboard.Object,
            bitmapAdapter,
            mockPictureRepo.Object,
            mockDrawStyleFactory.Object,
            transformUseCase.Object,
            new Mock<IScalingImageUseCase>().Object,
            transferToCanvasUseCase.Object,
            transferFromCanvasUseCase.Object,
            mockDrawingSessionProvider.Object,
            drawableCanvasVM,
            animationVM,
            drawingSessionVM,
            paletteVM,
            pictureIOService,
            new Mock<IThemeService>().Object,
            loadSettingsUseCase,
            saveSettingsUseCase,
            new WelcomeViewModel(mockSettingsRepoForUseCase.Object),
            () => new DockPictureViewModel(globalState, animationVM, bitmapAdapter, pictureIOService),
            () => new NewPictureWindowViewModel()
        );

        // DockPictureViewModel
        _dockViewModel = new DockPictureViewModel(globalState, animationVM, bitmapAdapter, pictureIOService);
        _dockViewModel.Initialize(Picture.CreateEmpty(new PictureSize(32, 32)), new FilePath("test.png"));

        // Setup Window and Container
        _window = new Window();
        _container = new PictureContainer
        {
            DataContext = _dockViewModel
        };
        _window.Content = _container;
    }

    [AvaloniaTest]
    public void Should_Display_Picture()
    {
        _window.Show();
        Assert.That(_container.DataContext, Is.Not.Null);
        Assert.That(_container.DataContext, Is.InstanceOf<DockPictureViewModel>());
    }
}
