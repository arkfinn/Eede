using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Eede.Application.Animations;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Moq;
using ReactiveUI;

namespace Eede.Presentation.Tests.Characterization;

public class BitmapManagementTests
{
    private GlobalState _globalState;
    private AnimationViewModel _animationViewModel;
    private Mock<IPictureRepository> _mockPictureRepository;
    private IPictureIOService _pictureIOService;
    private Mock<IBitmapAdapter<Bitmap>> _mockBitmapAdapter;

    [SetUp]
    public void Setup()
    {
        RxSchedulers.MainThreadScheduler = ReactiveUI.Avalonia.AvaloniaScheduler.Instance;
        _globalState = new GlobalState();
        AnimationPatternsProvider patternsProvider = new();
        _mockBitmapAdapter = new Mock<IBitmapAdapter<Bitmap>>();
        AnimationPatternService patternService = new(
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider));
        _animationViewModel = new AnimationViewModel(
            patternsProvider,
            patternService,
            new Mock<IFileSystem>().Object,
            _mockBitmapAdapter.Object);
        _mockPictureRepository = new Mock<IPictureRepository>();
        Mock<ISettingsRepository> mockSettingsRepository = new();
        _ = mockSettingsRepository.Setup(r => r.LoadAsync()).ReturnsAsync(new Eede.Application.Settings.AppSettings());
        _pictureIOService = new PictureIOService(
            new SavePictureUseCase(_mockPictureRepository.Object, mockSettingsRepository.Object),
            new LoadPictureUseCase(_mockPictureRepository.Object, mockSettingsRepository.Object));
    }

    [AvaloniaTest]
    public async Task DockPictureViewModel_ShouldUpdateBitmap()
    {
        WriteableBitmap mockBitmap = new(new Avalonia.PixelSize(1, 1), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888);
        bool mockCalled = false;
        _ = _mockBitmapAdapter.Setup(m => m.ConvertToPremultipliedBitmap(It.IsAny<Picture>()))
            .Callback(() => mockCalled = true)
            .Returns(mockBitmap);

        DockPictureViewModel viewModel = new(_globalState, _animationViewModel, _mockBitmapAdapter.Object, _pictureIOService)
        {
            // 手動で PictureBuffer をセットして発火を促す
            PictureBuffer = Picture.CreateEmpty(new PictureSize(32, 32))
        };
        viewModel.RaisePropertyChanged(nameof(viewModel.PictureBuffer));

        // 十分な時間待機
        for (int i = 0; i < 20 && !mockCalled; i++)
        {
            await Task.Delay(50);
        }

        Assert.That(mockCalled, Is.True, "Mock should be called");
        Assert.That(viewModel.PremultipliedBitmap, Is.Not.Null, "Initial bitmap should be set via mock");
    }

    [AvaloniaTest]
    public async Task DrawableCanvasViewModel_ShouldUpdateBitmap()
    {
        Mock<IInteractionCoordinator> mockCoordinator = new();
        Mock<IDrawingSessionProvider> mockSessionProvider = new();
        Mock<ISelectionService> mockSelectionService = new();
        Mock<IClipboard> mockClipboard = new();
        Mock<IAddFrameProvider> mockAddFrameProvider = new();

        DrawableCanvasViewModel viewModel = new(
            _globalState,
            mockAddFrameProvider.Object,
            mockClipboard.Object,
            _mockBitmapAdapter.Object,
            mockSessionProvider.Object,
            mockSelectionService.Object,
            mockCoordinator.Object);

        Picture dummyPicture = Picture.CreateEmpty(new PictureSize(32, 32));
        DrawingBuffer dummyBuffer = new(dummyPicture);
        _ = mockCoordinator.Setup(m => m.CurrentBuffer).Returns(dummyBuffer);
        _ = mockCoordinator.Setup(m => m.Painted(It.IsAny<DrawingBuffer>(), It.IsAny<PenStyle>(), It.IsAny<IImageTransfer>()))
            .Returns(dummyPicture);

        WriteableBitmap mockBitmap = new(new Avalonia.PixelSize(1, 1), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888);
        _ = _mockBitmapAdapter.Setup(m => m.ConvertToPremultipliedBitmap(It.IsAny<Picture>()))
            .Returns(mockBitmap);

        // Trigger update by notifying coordinator change
        mockCoordinator.Raise(m => m.StateChanged += null);

        for (int i = 0; i < 20 && viewModel.MyBitmap == null; i++)
        {
            await Task.Delay(50);
        }

        Assert.That(viewModel.MyBitmap, Is.Not.Null);
    }

    [AvaloniaTest]
    public async Task AnimationViewModel_ShouldUpdatePreviewBitmap()
    {
        WriteableBitmap mockBitmap = new(new Avalonia.PixelSize(1, 1), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888);
        bool mockCalled = false;
        _ = _mockBitmapAdapter.Setup(m => m.ConvertToPremultipliedBitmap(It.IsAny<Picture>()))
            .Callback(() => mockCalled = true)
            .Returns(mockBitmap);

        _animationViewModel.ActivePicture = Picture.CreateEmpty(new PictureSize(64, 64));
        AnimationPattern pattern = new("Test",
            [
                new AnimationFrame(0, 100)
            ], new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0));
        _animationViewModel.Patterns.Add(pattern);
        _animationViewModel.SelectedPattern = pattern;
        _animationViewModel.CurrentFrameIndex = 0;

        // 手動で通知
        _animationViewModel.RaisePropertyChanged(nameof(_animationViewModel.SelectedPattern));
        _animationViewModel.RaisePropertyChanged(nameof(_animationViewModel.ActivePicture));

        // CurrentFrame の更新を待つ
        for (int i = 0; i < 20 && _animationViewModel.CurrentFrame == null; i++)
        {
            await Task.Delay(50);
        }

        for (int i = 0; i < 20 && !mockCalled; i++)
        {
            await Task.Delay(50);
        }

        Assert.That(_animationViewModel.CurrentFrame, Is.Not.Null, "CurrentFrame should be set");
        Assert.That(mockCalled, Is.True, "Mock should be called for AnimationViewModel");
        Assert.That(_animationViewModel.PreviewBitmap, Is.Not.Null);
        Bitmap? initialBitmap = _animationViewModel.PreviewBitmap;

        WriteableBitmap secondBitmap = new(new Avalonia.PixelSize(1, 1), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888);
        mockCalled = false;
        _ = _mockBitmapAdapter.Setup(m => m.ConvertToPremultipliedBitmap(It.IsAny<Picture>()))
            .Callback(() => mockCalled = true)
            .Returns(secondBitmap);

        // Change magnification to update bitmap
        _animationViewModel.Magnification = new Magnification(2);
        _animationViewModel.RaisePropertyChanged(nameof(_animationViewModel.Magnification));

        for (int i = 0; i < 20 && !mockCalled; i++)
        {
            await Task.Delay(50);
        }

        Assert.That(mockCalled, Is.True, "Mock should be called again after property change");
        Assert.That(_animationViewModel.PreviewBitmap, Is.SameAs(secondBitmap));
    }
}