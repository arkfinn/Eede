using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Pictures;
using Eede.Application.Infrastructure;
using Eede.Application.UseCase.Pictures;
using Eede.Application.UseCase.Animations;
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

        // 2. Sub ViewModels Dependencies
        var patternsProvider = new AnimationPatternsProvider();
        var animationVM = new AnimationViewModel(
            patternsProvider,
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider),
            new Mock<IFileSystem>().Object);

        var mockDrawingSessionProvider = new Mock<IDrawingSessionProvider>();
        mockDrawingSessionProvider.Setup(x => x.CurrentSession).Returns(new DrawingSession(Picture.CreateEmpty(new PictureSize(1, 1))));
        // DrawingSessionViewModel setup (simplified)
        var drawingSessionVM = new DrawingSessionViewModel(mockDrawingSessionProvider.Object); 

        var mockClipboard = new Mock<IClipboard>();
        var mockCoordinator = new Mock<IInteractionCoordinator>();
        var mockAddFrameProvider = new Mock<IAddFrameProvider>();
        
        // UseCases
        var mockPictureRepo = new Mock<IPictureRepository>();
        var copyUseCase = new CopySelectionUseCase(mockClipboard.Object);
        var cutUseCase = new CutSelectionUseCase(mockClipboard.Object);
        var pasteUseCase = new PasteFromClipboardUseCase(mockClipboard.Object, Mock.Of<IDrawingSessionProvider>());
        var saveUseCase = new SavePictureUseCase(mockPictureRepo.Object);
        var loadUseCase = new LoadPictureUseCase(mockPictureRepo.Object);
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
            copyUseCase,
            cutUseCase,
            pasteUseCase,
            mockCoordinator.Object
        );

        var paletteVM = new PaletteContainerViewModel();
        var mockDrawStyleFactory = new Mock<IDrawStyleFactory>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        // MainViewModel
        _mainViewModel = new MainViewModel(
            globalState,
            mockClipboard.Object,
            bitmapAdapter,
            mockPictureRepo.Object,
            mockDrawStyleFactory.Object,
            transformUseCase.Object,
            transferToCanvasUseCase.Object,
            transferFromCanvasUseCase.Object,
            mockDrawingSessionProvider.Object,
            drawableCanvasVM,
            animationVM,
            drawingSessionVM,
            paletteVM,
            saveUseCase,
            loadUseCase,
            mockServiceProvider.Object
        );

        // DockPictureViewModel
        _dockViewModel = new DockPictureViewModel(globalState, animationVM, bitmapAdapter, saveUseCase, loadUseCase);
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