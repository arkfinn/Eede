using Eede.Application.Animations;
using Eede.Application.Services;
using Eede.Application.Pictures;
using Eede.Application.Drawings;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Settings;
using Eede.Presentation.Services;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.Common.Services;
using Eede.Presentation.Common.Adapters;
using Eede.Application.UseCase.Pictures;
using Moq;
using NUnit.Framework;
using Avalonia.Headless.NUnit;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System;
using System.Linq;

namespace Eede.Presentation.Tests.ViewModels.Pages;

public class MainViewModelIntegrationTests
{
    private GlobalState _globalState;
    private Mock<IAnimationService> _mockAnimationService;
    private Mock<IClipboardService> _mockClipboardService;
    private Mock<IBitmapAdapter<Avalonia.Media.Imaging.Bitmap>> _mockBitmapAdapter;
    private Mock<IPictureRepository> _mockPictureRepository;
    private Mock<IDrawStyleFactory> _mockDrawStyleFactory;
    private Mock<ITransformImageUseCase> _mockTransformImageUseCase;
    private Mock<ITransferImageToCanvasUseCase> _mockTransferImageToCanvasUseCase;
    private Mock<ITransferImageFromCanvasUseCase> _mockTransferImageFromCanvasUseCase;
    private Mock<IDrawingSessionProvider> _mockDrawingSessionProvider;
    private Mock<IServiceProvider> _mockServiceProvider;

    private DrawingSession _currentSession;

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
        _mockTransformImageUseCase = new Mock<ITransformImageUseCase>();
        _mockTransferImageToCanvasUseCase = new Mock<ITransferImageToCanvasUseCase>();
        _mockTransferImageFromCanvasUseCase = new Mock<ITransferImageFromCanvasUseCase>();
        _mockDrawingSessionProvider = new Mock<IDrawingSessionProvider>();
        
        _currentSession = new DrawingSession(Picture.CreateEmpty(new PictureSize(32, 32)));
        _mockDrawingSessionProvider.Setup(p => p.CurrentSession).Returns(() => _currentSession);
        _mockDrawingSessionProvider.Setup(p => p.Update(It.IsAny<DrawingSession>())).Callback<DrawingSession>(s => 
        {
            _currentSession = s;
            _mockDrawingSessionProvider.Raise(p => p.SessionChanged += null, s);
        });

        _mockDrawStyleFactory.Setup(f => f.Create(It.IsAny<DrawStyleType>())).Returns(new FreeCurve());

        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceProvider.Setup(s => s.GetService(typeof(DockPictureViewModel))).Returns(() => 
            new DockPictureViewModel(
                _globalState, 
                new AnimationViewModel(_mockAnimationService.Object, new Mock<IFileSystem>().Object),
                _mockBitmapAdapter.Object,
                new SavePictureUseCase(_mockPictureRepository.Object),
                new LoadPictureUseCase(_mockPictureRepository.Object)
            )
        );
    }

    private MainViewModel CreateViewModel()
    {
        var animationViewModel = new AnimationViewModel(_mockAnimationService.Object, new Mock<IFileSystem>().Object);
        var drawableCanvasViewModel = new DrawableCanvasViewModel(
            _globalState,
            new Mock<IAddFrameProvider>().Object,
            _mockClipboardService.Object,
            _mockBitmapAdapter.Object,
            _mockDrawingSessionProvider.Object,
            new CopySelectionUseCase(_mockClipboardService.Object),
            new CutSelectionUseCase(_mockClipboardService.Object),
            new PasteFromClipboardUseCase(_mockClipboardService.Object),
            new Mock<IInteractionCoordinator>().Object);
        var drawingSessionViewModel = new DrawingSessionViewModel(_mockDrawingSessionProvider.Object);
        var paletteContainerViewModel = new PaletteContainerViewModel();

        return new MainViewModel(
            _globalState,
            _mockClipboardService.Object,
            _mockBitmapAdapter.Object,
            _mockPictureRepository.Object,
            _mockDrawStyleFactory.Object,
            _mockTransformImageUseCase.Object,
            _mockTransferImageToCanvasUseCase.Object,
            _mockTransferImageFromCanvasUseCase.Object,
            _mockDrawingSessionProvider.Object,
            drawableCanvasViewModel,
            animationViewModel,
            drawingSessionViewModel,
            paletteContainerViewModel,
            new SavePictureUseCase(_mockPictureRepository.Object),
            new LoadPictureUseCase(_mockPictureRepository.Object),
            _mockServiceProvider.Object);
    }

    [AvaloniaTest]
    public void UndoPullAction_RestoresDockPicture()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = CreateViewModel();

            // 1. Setup: Add a DockPicture
            var picture = Picture.CreateEmpty(new PictureSize(32, 32));
            var dockPicture = (DockPictureViewModel)_mockServiceProvider.Object.GetService(typeof(DockPictureViewModel));
            dockPicture.Initialize(picture, Eede.Domain.Files.FilePath.Empty());
            viewModel.Pictures.Add(dockPicture);

            // 2. Mock Pull action effect
            var imageData = new byte[32 * 4 * 32];
            imageData[0] = 255; // Blue
            var updatedPicture = Picture.Create(new PictureSize(32, 32), imageData);
            _mockTransferImageFromCanvasUseCase.Setup(u => u.Execute(It.IsAny<Picture>(), It.IsAny<Picture>(), It.IsAny<Position>(), It.IsAny<IImageBlender>()))
                .Returns(updatedPicture);

            // 3. Perform Pull
            // 内部的に MainViewModel.OnPullFromDrawArea が呼ばれるはず
            dockPicture.OnPicturePull.Execute(new Position(0, 0)).Subscribe();
            scheduler.AdvanceByMs(10);
            
            // Assert: Dock picture updated
            Assert.That(dockPicture.PictureBuffer, Is.EqualTo(updatedPicture), $"Dock picture should be updated. Expected pixel[0]={updatedPicture.AsSpan()[0]}, Actual pixel[0]={dockPicture.PictureBuffer.AsSpan()[0]}");

            // Assert: Undo is enabled
            bool canUndo = false;
            viewModel.UndoCommand.CanExecute.Take(1).Subscribe(x => canUndo = x);
            Assert.That(canUndo, Is.True, "Undo should be enabled after Pull");

            // 4. Perform Undo
            bool executed = false;
            viewModel.UndoCommand.Execute().Subscribe(_ => executed = true, ex => throw ex);
            scheduler.AdvanceByMs(10);
            Assert.That(executed, Is.True, "Undo command should be executed");

            // Assert: Dock picture restored to initial state
            Assert.That(dockPicture.PictureBuffer, Is.EqualTo(picture), "Dock picture should be restored after Undo");

            // 5. Perform Redo
            bool redoExecuted = false;
            viewModel.RedoCommand.Execute().Subscribe(_ => redoExecuted = true, ex => throw ex);
            scheduler.AdvanceByMs(10);
            Assert.That(redoExecuted, Is.True, "Redo command should be executed");

            // Assert: Dock picture updated again after Redo
            Assert.That(dockPicture.PictureBuffer, Is.EqualTo(updatedPicture), "Dock picture should be updated again after Redo");
        });
    }
}
