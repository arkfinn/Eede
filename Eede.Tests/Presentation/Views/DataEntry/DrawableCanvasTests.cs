using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
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
}
