using Avalonia;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Pictures;
using Eede.Application.Services;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.DataEntry;
using Moq;
using NUnit.Framework;
using System;
using System.Reactive;

namespace Eede.Presentation.Tests.ViewModels.DataEntry;

[TestFixture]
public class DrawableCanvasCharacterizationTests
{
    private GlobalState _globalState;
    private Mock<IAddFrameProvider> _addFrameProviderMock;
    private Mock<IClipboardService> _clipboardServiceMock;
    private AvaloniaBitmapAdapter _bitmapAdapter;
    private DrawingSessionProvider _drawingSessionProvider;
    private CopySelectionUseCase _copySelectionUseCase;
    private CutSelectionUseCase _cutSelectionUseCase;
    private PasteFromClipboardUseCase _pasteFromClipboardUseCase;
    private InteractionCoordinator _coordinator;

    [SetUp]
    public void SetUp()
    {
        _globalState = new GlobalState();
        _addFrameProviderMock = new Mock<IAddFrameProvider>();
        _clipboardServiceMock = new Mock<IClipboardService>();
        _bitmapAdapter = new AvaloniaBitmapAdapter();
        _drawingSessionProvider = new DrawingSessionProvider();
        _copySelectionUseCase = new CopySelectionUseCase(_clipboardServiceMock.Object);
        _cutSelectionUseCase = new CutSelectionUseCase(_clipboardServiceMock.Object);
        _pasteFromClipboardUseCase = new PasteFromClipboardUseCase(_clipboardServiceMock.Object);
        _coordinator = new InteractionCoordinator(_drawingSessionProvider);
    }

    private DrawableCanvasViewModel CreateViewModel()
    {
        return new DrawableCanvasViewModel(
            _globalState,
            _addFrameProviderMock.Object,
            _clipboardServiceMock.Object,
            _bitmapAdapter,
            _drawingSessionProvider,
            _copySelectionUseCase,
            _cutSelectionUseCase,
            _pasteFromClipboardUseCase,
            _coordinator);
    }

    [AvaloniaTest]
    public void FreeCurve_Drawing_GoldenMaster()
    {
        var vm = CreateViewModel();
        vm.Magnification = new Magnification(1);
        vm.DrawStyle = new FreeCurve();
        vm.PenColor = new ArgbColor(255, 255, 0, 0); // Red
        vm.PenSize = 1;

        // Draw a L-shape
        vm.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
        vm.DrawingCommand.Execute(new Position(0, 1)).Subscribe();
        vm.DrawingCommand.Execute(new Position(1, 1)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(1, 1)).Subscribe();

        var picture = vm.PictureBuffer.Fetch();
        Assert.That(picture.PickColor(new Position(0, 0)), Is.EqualTo(vm.PenColor), "0,0 should be red");
        Assert.That(picture.PickColor(new Position(0, 1)), Is.EqualTo(vm.PenColor), "0,1 should be red");
        Assert.That(picture.PickColor(new Position(1, 1)), Is.EqualTo(vm.PenColor), "1,1 should be red");
        Assert.That(picture.PickColor(new Position(1, 0)).Alpha, Is.EqualTo(0), "1,0 should be transparent");
    }

    [AvaloniaTest]
    public void RegionSelector_Selection_GoldenMaster()
    {
        var vm = CreateViewModel();
        vm.Magnification = new Magnification(1);
        var selector = new RegionSelector();
        vm.DrawStyle = selector;

        // Select (2, 2) to (5, 5)
        vm.DrawBeginCommand.Execute(new Position(2, 2)).Subscribe();
        vm.DrawingCommand.Execute(new Position(5, 5)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(5, 5)).Subscribe();

        Assert.That(vm.IsRegionSelecting, Is.True);
        // From (2, 2) to (5, 5) means top-left (2, 2), bottom-right (5, 5). 
        // PictureArea.FromPosition calculates size as Math.Abs(to - from), so it is (3, 3).
        Assert.That(vm.SelectingArea, Is.EqualTo(new PictureArea(new Position(2, 2), new PictureSize(3, 3))));
    }

    [AvaloniaTest]
    public void Drawing_Cancel_GoldenMaster()
    {
        var vm = CreateViewModel();
        vm.Magnification = new Magnification(1);
        vm.DrawStyle = new FreeCurve();
        vm.PenColor = new ArgbColor(255, 0, 255, 0); // Green

        // Start drawing
        vm.DrawBeginCommand.Execute(new Position(5, 5)).Subscribe();
        vm.DrawingCommand.Execute(new Position(6, 6)).Subscribe();
        
        // Right click to cancel
        vm.PointerRightButtonPressedCommand.Execute(new Position(6, 6)).Subscribe();

        Assert.That(vm.PictureBuffer.IsDrawing(), Is.False);
        var picture = vm.PictureBuffer.Fetch();
        Assert.That(picture.PickColor(new Position(5, 5)).Alpha, Is.EqualTo(0), "5,5 should be transparent after cancel");
    }
}
