using Moq;
using NUnit.Framework;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Avalonia.Media.Imaging;
using Eede.Presentation.Common.Adapters;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Animations;
using Eede.Application.Animations;
using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Headless.NUnit;

namespace Eede.Presentation.Tests.ViewModels.Pages;

#nullable enable

[TestFixture]
public class MainViewModelTests
{
    private Mock<IClipboard> _clipboardMock = default!;
    private Mock<IBitmapAdapter<Bitmap>> _bitmapAdapterMock = default!;
    private Mock<IPictureRepository> _pictureRepositoryMock = default!;
    private Mock<IDrawStyleFactory> _drawStyleFactoryMock = default!;
    private Mock<ITransformImageUseCase> _transformImageUseCaseMock = default!;
    private Mock<IScalingImageUseCase> _scalingImageUseCaseMock = default!;
    private Mock<ITransferImageToCanvasUseCase> _transferImageToCanvasUseCaseMock = default!;
    private Mock<ITransferImageFromCanvasUseCase> _transferImageFromCanvasUseCaseMock = default!;
    private Mock<IDrawingSessionProvider> _drawingSessionProviderMock = default!;
    private Mock<IPictureIOService> _pictureIOServiceMock = default!;
    private Mock<IInteractionCoordinator> _interactionCoordinatorMock = default!;
    private Mock<IAddFrameProvider> _addFrameProviderMock = default!;
    private Mock<ISelectionService> _selectionServiceMock = default!;

    private Mock<IAnimationPatternsProvider> _patternsProviderMock = default!;
    private Mock<IAnimationPatternService> _animationPatternServiceMock = default!;
    private Mock<IFileSystem> _fileSystemMock = default!;

    private GlobalState _globalState = default!;
    private DrawableCanvasViewModel _drawableCanvasViewModel = default!;
    private AnimationViewModel _animationViewModel = default!;
    private DrawingSessionViewModel _drawingSessionViewModel = default!;
    private PaletteContainerViewModel _paletteContainerViewModel = default!;

    [SetUp]
    public void SetUp()
    {
        _clipboardMock = new Mock<IClipboard>();
        _bitmapAdapterMock = new Mock<IBitmapAdapter<Bitmap>>();
        _pictureRepositoryMock = new Mock<IPictureRepository>();
        _drawStyleFactoryMock = new Mock<IDrawStyleFactory>();
        _transformImageUseCaseMock = new Mock<ITransformImageUseCase>();
        _scalingImageUseCaseMock = new Mock<IScalingImageUseCase>();
        _transferImageToCanvasUseCaseMock = new Mock<ITransferImageToCanvasUseCase>();
        _transferImageFromCanvasUseCaseMock = new Mock<ITransferImageFromCanvasUseCase>();
        _drawingSessionProviderMock = new Mock<IDrawingSessionProvider>();
        var initialPicture = Picture.CreateEmpty(new PictureSize(32, 32));
        _drawingSessionProviderMock.Setup(x => x.CurrentSession).Returns(new DrawingSession(initialPicture));
        _pictureIOServiceMock = new Mock<IPictureIOService>();
        _interactionCoordinatorMock = new Mock<IInteractionCoordinator>();
        _addFrameProviderMock = new Mock<IAddFrameProvider>();
        _selectionServiceMock = new Mock<ISelectionService>();
        _patternsProviderMock = new Mock<IAnimationPatternsProvider>();
        _patternsProviderMock.Setup(x => x.Current).Returns(new AnimationPatterns());
        _animationPatternServiceMock = new Mock<IAnimationPatternService>();
        _fileSystemMock = new Mock<IFileSystem>();

        _globalState = new GlobalState();
        _animationViewModel = new AnimationViewModel(_patternsProviderMock.Object, _animationPatternServiceMock.Object, _fileSystemMock.Object, new AvaloniaBitmapAdapter());
        _drawingSessionViewModel = new DrawingSessionViewModel(_drawingSessionProviderMock.Object);
        _paletteContainerViewModel = new PaletteContainerViewModel();

        _drawableCanvasViewModel = new DrawableCanvasViewModel(
            _globalState,
            _addFrameProviderMock.Object,
            _clipboardMock.Object,
            _bitmapAdapterMock.Object,
            _drawingSessionProviderMock.Object,
            _selectionServiceMock.Object,
            _interactionCoordinatorMock.Object);
    }

    private MainViewModel CreateMainViewModel()
    {
        return new MainViewModel(
            _globalState,
            _clipboardMock.Object,
            _bitmapAdapterMock.Object,
            _pictureRepositoryMock.Object,
            _drawStyleFactoryMock.Object,
            _transformImageUseCaseMock.Object,
            _scalingImageUseCaseMock.Object,
            _transferImageToCanvasUseCaseMock.Object,
            _transferImageFromCanvasUseCaseMock.Object,
            _drawingSessionProviderMock.Object,
            _drawableCanvasViewModel,
            _animationViewModel,
            _drawingSessionViewModel,
            _paletteContainerViewModel,
            _pictureIOServiceMock.Object,
            () => new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _pictureIOServiceMock.Object),
            () => null!); // NewPictureWindowViewModel はここでは不要なため null!
    }

    [AvaloniaTest]
    public void OnPushToDrawArea_ShouldCallCommitSelectionWithTrue()
    {
        var mainVM = CreateMainViewModel();
        var dockPictureVM = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _pictureIOServiceMock.Object);
        mainVM.Pictures.Add(dockPictureVM);

        var area = new PictureArea(new Position(0, 0), new PictureSize(16, 16));
        var dummyPicture = Picture.CreateEmpty(new PictureSize(16, 16));
        _transferImageToCanvasUseCaseMock.Setup(x => x.Execute(It.IsAny<Picture>(), It.IsAny<PictureArea>()))
            .Returns(dummyPicture);

        // 初期化時の呼び出しをクリア
        _interactionCoordinatorMock.Invocations.Clear();

        // Act
        dockPictureVM.OnPicturePush.Execute(area).Subscribe();

        // Assert
        _interactionCoordinatorMock.Verify(x => x.CommitSelection(true), Times.AtLeastOnce, "CommitSelection(true) should be called before pushing image to draw area.");
    }

    [AvaloniaTest]
    public void OnPullFromDrawArea_ShouldCallCommitSelectionWithTrue()
    {
        var mainVM = CreateMainViewModel();
        var dockPictureVM = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _pictureIOServiceMock.Object);
        mainVM.Pictures.Add(dockPictureVM);

        var pos = new Position(10, 10);
        var dummyPicture = Picture.CreateEmpty(new PictureSize(32, 32));
        _transferImageFromCanvasUseCaseMock.Setup(x => x.Execute(It.IsAny<Picture>(), It.IsAny<Picture>(), It.IsAny<Position>(), It.IsAny<Eede.Domain.ImageEditing.Blending.IImageBlender>()))
            .Returns(dummyPicture);

        // 初期化時の呼び出しをクリア
        _interactionCoordinatorMock.Invocations.Clear();

        // Act
        dockPictureVM.OnPicturePull.Execute(pos).Subscribe();

        // Assert
        _interactionCoordinatorMock.Verify(x => x.CommitSelection(true), Times.AtLeastOnce, "CommitSelection(true) should be called before pulling image from draw area.");
    }

    [AvaloniaTest]
    public void GridFlagsInitialValueAndChangeTest()
    {
        var mainVM = CreateMainViewModel();

        Assert.Multiple(() =>
        {
            Assert.That(mainVM.IsShowPixelGrid, Is.False, "IsShowPixelGrid should be False by default");
            Assert.That(mainVM.IsShowCursorGrid, Is.False, "IsShowCursorGrid should be False by default");
        });

        mainVM.IsShowPixelGrid = true;
        Assert.That(mainVM.IsShowPixelGrid, Is.True);

        mainVM.IsShowCursorGrid = true;
        Assert.That(mainVM.IsShowCursorGrid, Is.True);
    }

    [AvaloniaTest]
    public void GridFlagPropagationAndVisibilityIntegrationTest()
    {
        var mainVM = CreateMainViewModel();
        var canvasVM = mainVM.DrawableCanvasViewModel;

        // 初期状態の確認
        Assert.Multiple(() =>
        {
            Assert.That(canvasVM.IsShowPixelGrid, Is.False);
            Assert.That(canvasVM.IsPixelGridEffectivelyVisible, Is.False);
            Assert.That(canvasVM.Magnification.Value, Is.EqualTo(4.0f), "Default mag should be x4");
        });

        // 1. MainViewModel で PixelGrid を ON にする -> 伝播して有効になる (x4なので)
        mainVM.IsShowPixelGrid = true;
        Assert.Multiple(() =>
        {
            Assert.That(canvasVM.IsShowPixelGrid, Is.True, "Flag should propagate to CanvasVM");
            Assert.That(canvasVM.IsPixelGridEffectivelyVisible, Is.True, "Should be visible at x4");
        });

        // 2. 倍率を x2 に下げる -> 内部フラグは ON のままだが、有効表示は False になる
        mainVM.Magnification = new Magnification(2);
        Assert.Multiple(() =>
        {
            Assert.That(canvasVM.IsShowPixelGrid, Is.True, "Flag should remain True");
            Assert.That(canvasVM.IsPixelGridEffectivelyVisible, Is.False, "Should be hidden below x4");
        });

        // 3. CursorGrid の伝播確認
        mainVM.IsShowCursorGrid = true;
        Assert.That(canvasVM.IsShowCursorGrid, Is.True);
        Assert.That(canvasVM.IsCursorGridEffectivelyVisible, Is.True, "Cursor grid should be effectively visible at any magnification");
    }

    [AvaloniaTest]
    public void CursorSizeInitializationTest()
    {
        var mainVM = CreateMainViewModel();
        var canvasVM = mainVM.DrawableCanvasViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(mainVM.CursorSize.Width, Is.EqualTo(32));
            Assert.That(canvasVM.CursorSize.Width, Is.EqualTo(32), "CursorSize should propagate to CanvasVM on initialization");
        });

        // 最小カーソルサイズの変更が CursorSize に反映されることを確認
        mainVM.MinCursorWidth = 16;
        mainVM.MinCursorHeight = 16;
        Assert.Multiple(() =>
        {
            Assert.That(mainVM.CursorSize.Width, Is.EqualTo(16));
            Assert.That(canvasVM.CursorSize.Width, Is.EqualTo(16));
        });
    }
}
