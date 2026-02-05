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
using Eede.Application.Infrastructure;
using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Headless.NUnit;

namespace Eede.Presentation.Tests.ViewModels.Pages;

[TestFixture]
public class MainViewModelTests
{
    private Mock<IClipboard> _clipboardMock;
    private Mock<IBitmapAdapter<Bitmap>> _bitmapAdapterMock;
    private Mock<IPictureRepository> _pictureRepositoryMock;
    private Mock<IDrawStyleFactory> _drawStyleFactoryMock;
    private Mock<ITransformImageUseCase> _transformImageUseCaseMock;
    private Mock<ITransferImageToCanvasUseCase> _transferImageToCanvasUseCaseMock;
    private Mock<ITransferImageFromCanvasUseCase> _transferImageFromCanvasUseCaseMock;
    private Mock<IDrawingSessionProvider> _drawingSessionProviderMock;
    private Mock<IPictureIOService> _pictureIOServiceMock;
    private Mock<IInteractionCoordinator> _interactionCoordinatorMock;
    private Mock<IAddFrameProvider> _addFrameProviderMock;
    private Mock<ISelectionService> _selectionServiceMock;

    private Mock<IAnimationPatternsProvider> _patternsProviderMock;
    private Mock<IAnimationPatternService> _animationPatternServiceMock;
    private Mock<IFileSystem> _fileSystemMock;

    private GlobalState _globalState;
    private DrawableCanvasViewModel _drawableCanvasViewModel;
    private AnimationViewModel _animationViewModel;
    private DrawingSessionViewModel _drawingSessionViewModel;
    private PaletteContainerViewModel _paletteContainerViewModel;

    [SetUp]
    public void SetUp()
    {
        _clipboardMock = new Mock<IClipboard>();
        _bitmapAdapterMock = new Mock<IBitmapAdapter<Bitmap>>();
        _pictureRepositoryMock = new Mock<IPictureRepository>();
        _drawStyleFactoryMock = new Mock<IDrawStyleFactory>();
        _transformImageUseCaseMock = new Mock<ITransformImageUseCase>();
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
        _animationViewModel = new AnimationViewModel(_patternsProviderMock.Object, _animationPatternServiceMock.Object, _fileSystemMock.Object);
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
            _transferImageToCanvasUseCaseMock.Object,
            _transferImageFromCanvasUseCaseMock.Object,
            _drawingSessionProviderMock.Object,
            _drawableCanvasViewModel,
            _animationViewModel,
            _drawingSessionViewModel,
            _paletteContainerViewModel,
            _pictureIOServiceMock.Object,
            () => new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _pictureIOServiceMock.Object),
            () => null); // NewPictureWindowViewModel はここでは不要
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
}
