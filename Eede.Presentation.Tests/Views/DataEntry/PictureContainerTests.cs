using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Pictures;
using Eede.Application.Services;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Services;
using Eede.Presentation.Common.Services;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Views.DataEntry;
using Moq;
using NUnit.Framework;
using System;

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
        var mockAnimationService = new Mock<IAnimationService>();
        mockAnimationService.Setup(x => x.Patterns).Returns(new System.Collections.Generic.List<AnimationPattern>());
        var mockFileSystem = new Mock<IFileSystem>();
        var animationVM = new AnimationViewModel(mockAnimationService.Object, mockFileSystem.Object);

        var mockDrawingSessionProvider = new Mock<IDrawingSessionProvider>();
        // DrawingSession mock setup
        var dummySession = new DrawingSession(Picture.CreateEmpty(new PictureSize(32, 32)));
        mockDrawingSessionProvider.Setup(x => x.CurrentSession).Returns(dummySession);

        var drawingSessionVM = new DrawingSessionViewModel(mockDrawingSessionProvider.Object);

        var paletteVM = new PaletteContainerViewModel();

        // DrawableCanvasViewModel Dependencies
        var mockAddFrameProvider = new Mock<IAddFrameProvider>();
        var mockClipboard = new Mock<IClipboardService>();
        var mockDrawAction = new Mock<IDrawActionUseCase>();
        
        var mockCopy = new Mock<CopySelectionUseCase>(mockClipboard.Object);
        var mockCut = new Mock<CutSelectionUseCase>(mockClipboard.Object);
        var mockPaste = new Mock<PasteFromClipboardUseCase>(mockClipboard.Object);
        var mockCoordinator = new Mock<IInteractionCoordinator>();

        var drawableCanvasVM = new DrawableCanvasViewModel(
            globalState,
            mockAddFrameProvider.Object,
            mockClipboard.Object,
            bitmapAdapter,
            mockDrawingSessionProvider.Object,
            mockCopy.Object,
            mockCut.Object,
            mockPaste.Object,
            mockCoordinator.Object
        );

        // 3. UseCases for MainViewModel
        var mockRepo = new Mock<IPictureRepository>();
        var saveUseCase = new SavePictureUseCase(mockRepo.Object);
        var loadUseCase = new LoadPictureUseCase(mockRepo.Object);
        var mockEditingUseCase = new Mock<IPictureEditingUseCase>();

        // 4. MainViewModel Dependencies
        var mockFactory = new Mock<IDrawStyleFactory>();
        var mockServices = new Mock<IServiceProvider>();

        mockServices.Setup(s => s.GetService(typeof(DockPictureViewModel)))
            .Returns(() => new DockPictureViewModel(
                globalState, 
                animationVM, 
                bitmapAdapter, 
                saveUseCase, 
                loadUseCase));

        // 5. MainViewModel
        _mainViewModel = new MainViewModel(
            globalState,
            mockClipboard.Object,
            bitmapAdapter,
            mockRepo.Object,
            mockFactory.Object,
            mockEditingUseCase.Object,
            mockDrawingSessionProvider.Object,
            drawableCanvasVM,
            animationVM,
            drawingSessionVM,
            paletteVM,
            saveUseCase,
            loadUseCase,
            mockServices.Object
        );

        // 6. DockPictureViewModel
        _dockViewModel = new DockPictureViewModel(
            globalState,
            animationVM,
            bitmapAdapter,
            saveUseCase,
            loadUseCase
        );
        var size = new PictureSize(200, 200);
        var data = new byte[200 * 200 * 4];
        var picture = Picture.Create(size, data);
        _dockViewModel.Initialize(picture, FilePath.Empty());

        _container = new PictureContainer
        {
            DataContext = _dockViewModel
        };

        _window = new Window { Content = _container };
        _window.Show();
        
        Dispatcher.UIThread.RunJobs();
    }

    [TearDown]
    public void TearDown()
    {
        _window.Close();
    }

    private Control GetCanvas()
    {
        return _container.FindControl<Panel>("canvas");
    }

    private Control GetCursor()
    {
        return _container.FindControl<Control>("cursor");
    }

    private static PointerPressedEventArgs CreatePressedArgs(IInputElement source, Point point, PointerUpdateKind kind)
    {
        var pointer = new Mock<IPointer>();
        return new PointerPressedEventArgs(
            source,
            pointer.Object,
            (Visual)source,
            point,
            0,
            new PointerPointProperties(RawInputModifiers.None, kind),
            KeyModifiers.None);
    }

    private static PointerEventArgs CreateMovedArgs(IInputElement source, Point point)
    {
        var pointer = new Mock<IPointer>();
        return new PointerEventArgs(
            InputElement.PointerMovedEvent,
            source,
            pointer.Object,
            (Visual)source,
            point,
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
            KeyModifiers.None);
    }

    private static PointerReleasedEventArgs CreateReleasedArgs(IInputElement source, Point point, PointerUpdateKind kind)
    {
        var pointer = new Mock<IPointer>();
        var button = kind == PointerUpdateKind.RightButtonReleased ? MouseButton.Right : MouseButton.Left;
        return new PointerReleasedEventArgs(
            source,
            pointer.Object,
            (Visual)source,
            point,
            0,
            new PointerPointProperties(RawInputModifiers.None, kind),
            KeyModifiers.None,
            button);
    }

    [AvaloniaTest]
    public void Cursor_Should_Follow_Mouse_With_Retained_Size_After_Selection()
    {
        var canvas = GetCanvas();
        var cursor = GetCursor();

        // Initial check (32x32)
        Assert.That(cursor.Width, Is.EqualTo(32));

        // 1. Right Drag: (0,0) -> (60,60) -> Snap to 64x64
        var startPos = new Point(0, 0);
        var endPos = new Point(60, 60);

        canvas.RaiseEvent(CreatePressedArgs(canvas, startPos, PointerUpdateKind.RightButtonPressed));
        canvas.RaiseEvent(CreateMovedArgs(canvas, endPos));
        
        Dispatcher.UIThread.RunJobs();
        Assert.That(cursor.Width, Is.EqualTo(64));

        // Release
        canvas.RaiseEvent(CreateReleasedArgs(canvas, endPos, PointerUpdateKind.RightButtonReleased));
        Dispatcher.UIThread.RunJobs();

        // 2. Cursor size should remain 64x64 after release
        Assert.That(cursor.Width, Is.EqualTo(64), "Cursor width should retain selected size.");
        Assert.That(cursor.Height, Is.EqualTo(64), "Cursor height should retain selected size.");

        // 3. Move Mouse to (80, 80)
        var movePos = new Point(80, 80);
        canvas.RaiseEvent(CreateMovedArgs(canvas, movePos));
        Dispatcher.UIThread.RunJobs();

        // 4. Verification
        // Position should follow mouse (80, 80) -> Snapped to 64 (Grid size 32)
        Assert.That(cursor.Margin.Left, Is.EqualTo(64), "Cursor should follow mouse X (snapped).");
        Assert.That(cursor.Margin.Top, Is.EqualTo(64), "Cursor should follow mouse Y (snapped).");

        // Size should STILL be 64x64 (NormalCursorState but with new size)
        Assert.That(cursor.Width, Is.EqualTo(64), "Cursor width should remain 64 after moving.");
        Assert.That(cursor.Height, Is.EqualTo(64), "Cursor height should remain 64 after moving.");
    }

    [AvaloniaTest]
    public void RightClick_Should_Start_New_Selection_Immediately()
    {
        var canvas = GetCanvas();
        var cursor = GetCursor();

        // 1. Do a selection to set cursor size to 64
        var startPos = new Point(0, 0);
        var endPos = new Point(60, 60); // Snaps to 64
        canvas.RaiseEvent(CreatePressedArgs(canvas, startPos, PointerUpdateKind.RightButtonPressed));
        canvas.RaiseEvent(CreateMovedArgs(canvas, endPos));
        canvas.RaiseEvent(CreateReleasedArgs(canvas, endPos, PointerUpdateKind.RightButtonReleased));
        Dispatcher.UIThread.RunJobs();

        Assert.That(cursor.Width, Is.EqualTo(64));

        // 2. Start NEW selection at (100, 100)
        var newStartPos = new Point(100, 100);
        canvas.RaiseEvent(CreatePressedArgs(canvas, newStartPos, PointerUpdateKind.RightButtonPressed));
        Dispatcher.UIThread.RunJobs();

        // Verification: Cursor should be at (100, 100) -> Snapped to 96
        Assert.That(cursor.Margin.Left, Is.EqualTo(96));
        Assert.That(cursor.Margin.Top, Is.EqualTo(96));
        
        // And dragging from here should update size based on (100,100) as start
        var newDragPos = new Point(160, 160); // Delta=60 -> Size=64
        canvas.RaiseEvent(CreateMovedArgs(canvas, newDragPos));
        Dispatcher.UIThread.RunJobs();

        Assert.That(cursor.Width, Is.EqualTo(64));
    }
}