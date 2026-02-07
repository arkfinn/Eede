using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Eede.Application.Animations;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using ReactiveUI;
using ReactiveUI.Testing;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

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
        RxApp.MainThreadScheduler = Avalonia.ReactiveUI.AvaloniaScheduler.Instance;
        _globalState = new GlobalState();
        var patternsProvider = new AnimationPatternsProvider();
        _mockBitmapAdapter = new Mock<IBitmapAdapter<Bitmap>>();
        var patternService = new AnimationPatternService(
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider));
        _animationViewModel = new AnimationViewModel(
            patternsProvider,
            patternService,
            new Mock<IFileSystem>().Object,
            _mockBitmapAdapter.Object);
        _mockPictureRepository = new Mock<IPictureRepository>();
        _pictureIOService = new PictureIOService(
            new SavePictureUseCase(_mockPictureRepository.Object),
            new LoadPictureUseCase(_mockPictureRepository.Object));
    }

    [AvaloniaTest]
    public async Task DockPictureViewModel_ShouldUpdateBitmap()
    {
        var mockBitmap = new WriteableBitmap(new Avalonia.PixelSize(1, 1), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888);
        bool mockCalled = false;
        _mockBitmapAdapter.Setup(m => m.ConvertToPremultipliedBitmap(It.IsAny<Picture>()))
            .Callback(() => mockCalled = true)
            .Returns(mockBitmap);

        var viewModel = new DockPictureViewModel(_globalState, _animationViewModel, _mockBitmapAdapter.Object, _pictureIOService);

        // 手動で PictureBuffer をセットして発火を促す
        viewModel.PictureBuffer = Picture.CreateEmpty(new PictureSize(32, 32));
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
        var mockCoordinator = new Mock<IInteractionCoordinator>();
        var mockSessionProvider = new Mock<IDrawingSessionProvider>();
        var mockSelectionService = new Mock<ISelectionService>();
        var mockClipboard = new Mock<IClipboard>();
        var mockAddFrameProvider = new Mock<IAddFrameProvider>();

        var viewModel = new DrawableCanvasViewModel(
            _globalState,
            mockAddFrameProvider.Object,
            mockClipboard.Object,
            _mockBitmapAdapter.Object,
            mockSessionProvider.Object,
            mockSelectionService.Object,
            mockCoordinator.Object);

        var dummyPicture = Picture.CreateEmpty(new PictureSize(32, 32));
        var dummyBuffer = new DrawingBuffer(dummyPicture);
        mockCoordinator.Setup(m => m.CurrentBuffer).Returns(dummyBuffer);
        mockCoordinator.Setup(m => m.Painted(It.IsAny<DrawingBuffer>(), It.IsAny<PenStyle>(), It.IsAny<IImageTransfer>()))
            .Returns(dummyPicture);

        var mockBitmap = new WriteableBitmap(new Avalonia.PixelSize(1, 1), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888);
        _mockBitmapAdapter.Setup(m => m.ConvertToPremultipliedBitmap(It.IsAny<Picture>()))
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
        var mockBitmap = new WriteableBitmap(new Avalonia.PixelSize(1, 1), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888);
        bool mockCalled = false;
        _mockBitmapAdapter.Setup(m => m.ConvertToPremultipliedBitmap(It.IsAny<Picture>()))
            .Callback(() => mockCalled = true)
            .Returns(mockBitmap);

        _animationViewModel.ActivePicture = Picture.CreateEmpty(new PictureSize(64, 64));
        var pattern = new AnimationPattern("Test", new System.Collections.Generic.List<AnimationFrame>
            {
                new AnimationFrame(0, 100)
            }, new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0));
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
        var initialBitmap = _animationViewModel.PreviewBitmap;

        var secondBitmap = new WriteableBitmap(new Avalonia.PixelSize(1, 1), new Avalonia.Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888);
        mockCalled = false;
        _mockBitmapAdapter.Setup(m => m.ConvertToPremultipliedBitmap(It.IsAny<Picture>()))
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