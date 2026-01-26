using Eede.Application.Animations;
using Eede.Application.Services;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Application.Drawings;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.Common.Services;
using Moq;
using NUnit.Framework;
using Avalonia.Headless.NUnit;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using ReactiveUI;
using System.Reactive;

namespace Eede.Presentation.Tests.ViewModels.Pages;

public class MainViewModelTests
{
    private GlobalState _globalState;
    private Mock<IAnimationService> _mockAnimationService;
    private Mock<IClipboardService> _mockClipboardService;
    private Mock<IBitmapAdapter<Avalonia.Media.Imaging.Bitmap>> _mockBitmapAdapter;
    private Mock<IPictureRepository> _mockPictureRepository;
    private Mock<IDrawStyleFactory> _mockDrawStyleFactory;
    private Mock<IPictureEditingUseCase> _mockPictureEditingUseCase;
    private Mock<IDrawingSessionProvider> _mockDrawingSessionProvider;
    private Mock<IDrawActionUseCase> _mockDrawActionUseCase;

    [SetUp]
    public void Setup()
    {
        _globalState = new GlobalState();
        _mockAnimationService = new Mock<IAnimationService>();
        _mockAnimationService.Setup(s => s.Patterns).Returns(new System.Collections.Generic.List<Eede.Domain.Animations.AnimationPattern>());
        _mockClipboardService = new Mock<IClipboardService>();
        _mockBitmapAdapter = new Mock<IBitmapAdapter<Avalonia.Media.Imaging.Bitmap>>();
        _mockPictureRepository = new Mock<IPictureRepository>();
        _mockDrawStyleFactory = new Mock<IDrawStyleFactory>();
        _mockPictureEditingUseCase = new Mock<IPictureEditingUseCase>();
        _mockDrawingSessionProvider = new Mock<IDrawingSessionProvider>();
        _mockDrawingSessionProvider.Setup(p => p.CurrentSession).Returns(new DrawingSession(Picture.CreateEmpty(new PictureSize(32, 32))));
        _mockDrawActionUseCase = new Mock<IDrawActionUseCase>();

        // Default behavior for factory
        _mockDrawStyleFactory.Setup(f => f.Create(It.IsAny<DrawStyleType>())).Returns(new FreeCurve());
    }

    private MainViewModel CreateViewModel()
    {
        var animationViewModel = new AnimationViewModel(_mockAnimationService.Object, new Mock<IFileSystem>().Object);
        var drawableCanvasViewModel = new DrawableCanvasViewModel(_globalState, animationViewModel, _mockClipboardService.Object, _mockBitmapAdapter.Object, _mockDrawActionUseCase.Object, _mockDrawingSessionProvider.Object);
        var drawingSessionViewModel = new DrawingSessionViewModel(_mockDrawingSessionProvider.Object);
        var paletteContainerViewModel = new PaletteContainerViewModel();

        return new MainViewModel(
            _globalState,
            _mockClipboardService.Object,
            _mockBitmapAdapter.Object,
            _mockPictureRepository.Object,
            _mockDrawStyleFactory.Object,
            _mockPictureEditingUseCase.Object,
            _mockDrawingSessionProvider.Object,
            drawableCanvasViewModel,
            animationViewModel,
            drawingSessionViewModel,
            paletteContainerViewModel);
    }

    [AvaloniaTest]
    public void Characterization_Initialization()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = CreateViewModel();

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.Pictures, Is.Not.Null);
                Assert.That(viewModel.DrawableCanvasViewModel, Is.Not.Null);
                Assert.That(viewModel.AnimationViewModel, Is.Not.Null);
                Assert.That(viewModel.DrawingSessionViewModel, Is.Not.Null);
            });
        });
    }

    [AvaloniaTest]
    public void Characterization_UndoRedoState()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = CreateViewModel();

            // 初期状態ではUndo/Redo不可
            Assert.That(((System.Windows.Input.ICommand)viewModel.UndoCommand).CanExecute(null), Is.False);
            Assert.That(((System.Windows.Input.ICommand)viewModel.RedoCommand).CanExecute(null), Is.False);
        });
    }
}
