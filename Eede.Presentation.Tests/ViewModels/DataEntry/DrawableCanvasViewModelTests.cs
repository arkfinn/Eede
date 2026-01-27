using Avalonia;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Pictures;
using Eede.Application.Services;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.DataEntry;
using Moq;
using NUnit.Framework;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;

namespace Eede.Presentation.Tests.ViewModels.DataEntry;

[TestFixture]
public class DrawableCanvasViewModelTests
{
    private Mock<IAddFrameProvider> _addFrameProviderMock;
    private Mock<IClipboardService> _clipboardServiceMock;
    private Mock<IBitmapAdapter<Bitmap>> _bitmapAdapterMock;
    private Mock<IDrawActionUseCase> _drawActionUseCaseMock;
    private Mock<IDrawingSessionProvider> _drawingSessionProviderMock;
    private Mock<CopySelectionUseCase> _copySelectionUseCaseMock;
    private Mock<CutSelectionUseCase> _cutSelectionUseCaseMock;
    private Mock<PasteFromClipboardUseCase> _pasteFromClipboardUseCaseMock;
    private GlobalState _globalState;

    [SetUp]
    public void SetUp()
    {
        _addFrameProviderMock = new Mock<IAddFrameProvider>();
        _clipboardServiceMock = new Mock<IClipboardService>();
        _bitmapAdapterMock = new Mock<IBitmapAdapter<Bitmap>>();
        _drawActionUseCaseMock = new Mock<IDrawActionUseCase>();
        _drawingSessionProviderMock = new Mock<IDrawingSessionProvider>();
        _copySelectionUseCaseMock = new Mock<CopySelectionUseCase>(_clipboardServiceMock.Object);
        _cutSelectionUseCaseMock = new Mock<CutSelectionUseCase>(_clipboardServiceMock.Object);
        _pasteFromClipboardUseCaseMock = new Mock<PasteFromClipboardUseCase>(_clipboardServiceMock.Object);
        _globalState = new GlobalState();
    }

    private DrawableCanvasViewModel CreateViewModel()
    {
        return new DrawableCanvasViewModel(
            _globalState,
            _addFrameProviderMock.Object,
            _clipboardServiceMock.Object,
            _bitmapAdapterMock.Object,
            _drawActionUseCaseMock.Object,
            _drawingSessionProviderMock.Object,
            _copySelectionUseCaseMock.Object,
            _cutSelectionUseCaseMock.Object,
            _pasteFromClipboardUseCaseMock.Object);
    }

    [AvaloniaTest]
    public void SelectingArea_Calculation_Test()
    {
        var vm = CreateViewModel();
        vm.Magnification = new Magnification(4);
        vm.SelectingArea = new PictureArea(new Position(10, 20), new PictureSize(30, 40));

        // 10 * 4 = 40, 20 * 4 = 80
        Assert.That(vm.SelectingThickness, Is.EqualTo(new Thickness(40, 80, 0, 0)));
        // 30 * 4 = 120, 40 * 4 = 160
        Assert.That(vm.SelectingSize.Width, Is.EqualTo(120));
        Assert.That(vm.SelectingSize.Height, Is.EqualTo(160));
    }

    [AvaloniaTest]
    public void Preview_Calculation_Test()
    {
        var vm = CreateViewModel();
        vm.Magnification = new Magnification(2);
        vm.PreviewPosition = new Position(5, 15);
        // Pixelsをセットしないと計算が走らない
        vm.PreviewPixels = Picture.CreateEmpty(new PictureSize(10, 10));

        // 5 * 2 = 10, 15 * 2 = 30
        Assert.That(vm.PreviewThickness, Is.EqualTo(new Thickness(10, 30, 0, 0)));
        // 10 * 2 = 20
        Assert.That(vm.PreviewSize.Width, Is.EqualTo(20));
        Assert.That(vm.PreviewSize.Height, Is.EqualTo(20));

                // Raw

                Assert.That(vm.RawPreviewThickness, Is.EqualTo(new Thickness(5, 15, 0, 0)));

                Assert.That(vm.RawPreviewSize.Width, Is.EqualTo(10));

            }

        

                [AvaloniaTest]

        

                public void Drawing_Workflow_Test()

        

                {

        

                    var vm = CreateViewModel();

        

                    vm.Magnification = new Magnification(1);

        

                    var initialPicture = Picture.CreateEmpty(new PictureSize(32, 32));

        

                    vm.SetPicture(initialPicture);

        

            

        

                    bool drewEventFired = false;

        

                    vm.Drew += (previous, current, area1, area2) => { drewEventFired = true; };

        

            

        

                    // 描画開始 (10, 10)

        

                    vm.DrawBeginCommand.Execute(new Position(10, 10)).Subscribe();

        

                    Assert.That(vm.PictureBuffer.IsDrawing(), Is.True);

        

            

        

                    // 描画中 (11, 11)

        

                    vm.DrawingCommand.Execute(new Position(11, 11)).Subscribe();

        

            

        

                    // 描画終了 (11, 11)

        

                    vm.DrawEndCommand.Execute(new Position(11, 11)).Subscribe();

        

            

        

                    Assert.That(vm.PictureBuffer.IsDrawing(), Is.False);

        

                    Assert.That(drewEventFired, Is.True, "Drew event should be fired after drawing");

        

                }

        

            

        

            [AvaloniaTest]

            public void CopyCommand_ShouldInvokeClipboardService()

            {

                var vm = CreateViewModel();

                vm.SetPicture(Picture.CreateEmpty(new PictureSize(32, 32)));

        

                vm.CopyCommand.Execute().Subscribe();

        

                _clipboardServiceMock.Verify(x => x.CopyAsync(It.IsAny<Picture>()), Times.Once);

            }

        }

        