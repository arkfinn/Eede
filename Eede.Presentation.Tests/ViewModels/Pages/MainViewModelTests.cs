using Eede.Application.Animations;
using Eede.Application.Services;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Pages;
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

    [SetUp]
    public void Setup()
    {
        _globalState = new GlobalState();
        _mockAnimationService = new Mock<IAnimationService>();
        _mockAnimationService.Setup(s => s.Patterns).Returns(new System.Collections.Generic.List<Eede.Domain.Animations.AnimationPattern>());
        _mockClipboardService = new Mock<IClipboardService>();
        _mockBitmapAdapter = new Mock<IBitmapAdapter<Avalonia.Media.Imaging.Bitmap>>();
        _mockPictureRepository = new Mock<IPictureRepository>();
    }

    private MainViewModel CreateViewModel()
    {
        return new MainViewModel(
            _globalState,
            _mockAnimationService.Object,
            _mockClipboardService.Object,
            _mockBitmapAdapter.Object,
            _mockPictureRepository.Object);
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
