using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Eede.Application.Animations;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Coordinators;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.Views.DataEntry;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace Eede.Presentation.Tests.Views.DataEntry;

[TestFixture]
public class DrawableCanvasTests
{
    private DrawableCanvas _canvas;
    private DrawableCanvasViewModel _viewModel;
    private Window _window;

    [SetUp]
    public void Setup()
    {
        var bitmapAdapter = new AvaloniaBitmapAdapter();
        var globalState = new GlobalState();
        var mockDrawingSessionProvider = new Mock<IDrawingSessionProvider>();
        mockDrawingSessionProvider.Setup(x => x.CurrentSession)
            .Returns(new DrawingSession(Picture.CreateEmpty(new PictureSize(32, 32))));

        var mockClipboard = new Mock<IClipboard>();
        var mockCoordinator = new Mock<IInteractionCoordinator>();
        var mockAddFrameProvider = new Mock<IAddFrameProvider>();

        var copyUseCase = new CopySelectionUseCase(mockClipboard.Object);
        var cutUseCase = new CutSelectionUseCase(mockClipboard.Object);
        var pasteUseCase = new PasteFromClipboardUseCase(mockClipboard.Object, mockDrawingSessionProvider.Object);
        var selectionClipboard = new SelectionClipboard(copyUseCase, cutUseCase, pasteUseCase);

        _viewModel = new DrawableCanvasViewModel(
            globalState,
            mockAddFrameProvider.Object,
            mockClipboard.Object,
            bitmapAdapter,
            mockDrawingSessionProvider.Object,
            selectionClipboard,
            mockCoordinator.Object
        );

        _canvas = new DrawableCanvas
        {
            DataContext = _viewModel
        };

        _window = new Window
        {
            Content = _canvas,
            Width = 800,
            Height = 600
        };
    }

    [TearDown]
    public void TearDown()
    {
        _window.Close();
    }

    [AvaloniaTest]
    public void Should_HaveOutsideTilePanel_WithOpaqueBackground()
    {
        _canvas.ApplyTemplate();
        _window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var allChildren = _canvas.GetVisualDescendants();
        var outsideTilePanel = allChildren.OfType<Panel>().FirstOrDefault(p => p.Name == "outsideTilePanel");

        Assert.That(outsideTilePanel, Is.Not.Null, "outsideTilePanel が存在する必要があります");
        var outsideBrush = outsideTilePanel!.Background as ISolidColorBrush;
        Assert.That(outsideBrush != null && outsideBrush.Color.A == 255,
            "outsideTilePanel の背景は不透明であるべきです。");
    }

    [AvaloniaTest]
    public void Should_CalculateOverscrollMargin_OnViewportChange()
    {
        _canvas.ApplyTemplate();
        _window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var allChildren = _canvas.GetVisualDescendants();
        var scrollViewer = allChildren.OfType<ScrollViewer>().FirstOrDefault(s => s.Name == "scrollViewer");
        var canvasGrid = allChildren.OfType<Grid>().FirstOrDefault(g => g.Name == "canvasGrid");

        Assert.That(scrollViewer, Is.Not.Null, "scrollViewer が存在する必要があります");
        Assert.That(canvasGrid, Is.Not.Null, "canvasGrid が存在する必要があります");

        // Viewport がある場合、Margin が正の値に設定されること
        var margin = canvasGrid!.Margin;
        Assert.That(margin.Left, Is.GreaterThan(0.0), "Left margin should be greater than zero.");
        Assert.That(margin.Top, Is.GreaterThan(0.0), "Top margin should be greater than zero.");
    }

    [AvaloniaTest]
    public void CenterContent_Should_SetOffset_ToCenter()
    {
        _canvas.ApplyTemplate();
        _window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var allChildren = _canvas.GetVisualDescendants();
        var scrollViewer = allChildren.OfType<ScrollViewer>().FirstOrDefault(s => s.Name == "scrollViewer");

        Assert.That(scrollViewer, Is.Not.Null);

        _canvas.CenterContent();

        // オフセットが設定され、初期センタリングが行われること
        Assert.That(scrollViewer!.Offset.X, Is.GreaterThanOrEqualTo(0.0));
        Assert.That(scrollViewer.Offset.Y, Is.GreaterThanOrEqualTo(0.0));
    }

    [AvaloniaTest]
    public void MiddleButtonDrag_OnCanvas_ShouldPanScrollViewer()
    {
        _viewModel.SetPicture(Picture.CreateEmpty(new PictureSize(256, 256)));
        _canvas.ApplyTemplate();
        _window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var scrollViewer = _canvas.GetVisualDescendants().OfType<ScrollViewer>().First(s => s.Name == "scrollViewer");
        var initialOffset = scrollViewer.Offset;

        // キャンバス上の中央付近で中ボタン押下 -> ドラッグ移動 -> リリース
        var startPoint = new Point(400, 300);
        var movePoint = new Point(350, 240); // (-50, -60) ドラッグ -> Offset は (+50, +60) 増えるはず

        _window.MouseDown(startPoint, Avalonia.Input.MouseButton.Middle);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        _window.MouseMove(movePoint, RawInputModifiers.MiddleMouseButton);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        _window.MouseUp(movePoint, Avalonia.Input.MouseButton.Middle);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.That(scrollViewer.Offset.X, Is.EqualTo(initialOffset.X + 50).Within(1.0), "中ボタンドラッグでX方向にスクロールするべき");
        Assert.That(scrollViewer.Offset.Y, Is.EqualTo(initialOffset.Y + 60).Within(1.0), "中ボタンドラッグでY方向にスクロールするべき");
    }

    [AvaloniaTest]
    public void LargePicture_ShouldAlignTopLeft_BothXAndY()
    {
        // MainViewの左ペインと同様に縦長ビューポート(480x650)で800x800表示(200x200 @ 4x)の画像を初期表示
        _window.Width = 480;
        _window.Height = 650;
        _viewModel.SetPicture(Picture.CreateEmpty(new PictureSize(200, 200)));

        _canvas.ApplyTemplate();
        _window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        _window.UpdateLayout();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var scrollViewer = _canvas.GetVisualDescendants().OfType<ScrollViewer>().First(s => s.Name == "scrollViewer");
        var canvasGrid = _canvas.GetVisualDescendants().OfType<Grid>().First(g => g.Name == "canvasGrid");

        Assert.That(scrollViewer.Offset.X, Is.EqualTo(canvasGrid.Margin.Left).Within(0.5), "大きい画像の初期Xオフセットは Margin.Left (画像原点0) であるべき");
        Assert.That(scrollViewer.Offset.Y, Is.EqualTo(canvasGrid.Margin.Top).Within(0.5), "大きい画像の初期Yオフセットは Margin.Top (画像原点0) であるべき");
    }

    [AvaloniaTest]
    public void LeftButtonDrag_FromOutsideTilePanel_IntoCanvas_ShouldPropagateCommandsWithNegativeCoordinates()
    {
        Position? pressedPos = null;
        Position? movedPos = null;
        Position? releasedPos = null;
        _canvas.PointerLeftButtonPressedCommand = ReactiveUI.ReactiveCommand.Create<Position>(p => pressedPos = p);
        _canvas.PointerMovedCommand = ReactiveUI.ReactiveCommand.Create<Position>(p => movedPos = p);
        _canvas.PointerLeftButtonReleasedCommand = ReactiveUI.ReactiveCommand.Create<Position>(p => releasedPos = p);

        _canvas.ApplyTemplate();
        _window.Show();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var canvasImage = _canvas.GetVisualDescendants().OfType<Image>().First(i => i.Name == "canvas");
        var canvasOriginInWindow = canvasImage.TranslatePoint(new Point(0, 0), _window)!.Value;

        var startInWindow = new Point(canvasOriginInWindow.X - 10, canvasOriginInWindow.Y - 8);
        var endInWindow = new Point(canvasOriginInWindow.X + 12, canvasOriginInWindow.Y + 15);

        _window.MouseDown(startInWindow, MouseButton.Left);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        _window.MouseMove(endInWindow, RawInputModifiers.LeftMouseButton);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        _window.MouseUp(endInWindow, MouseButton.Left);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Multiple(() =>
        {
            Assert.That(pressedPos, Is.EqualTo(new Position(-10, -8)));
            Assert.That(movedPos, Is.EqualTo(new Position(12, 15)));
            Assert.That(releasedPos, Is.EqualTo(new Position(12, 15)));
        });
    }
}
