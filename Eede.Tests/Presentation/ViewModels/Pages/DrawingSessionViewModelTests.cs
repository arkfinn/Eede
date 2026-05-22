using Avalonia.Headless.NUnit;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.ViewModels.Pages;
using Microsoft.Reactive.Testing;
using Moq;
using ReactiveUI;
using ReactiveUI.Testing;

namespace Eede.Presentation.Tests.ViewModels.Pages;

public class DrawingSessionViewModelTests
{
    private Picture _initialPicture;
    private readonly PictureSize _size = new(32, 32);
    private Mock<IDrawingSessionProvider> _mockProvider;

    [SetUp]
    public void Setup()
    {
        _initialPicture = Picture.CreateEmpty(_size);
        _mockProvider = new Mock<IDrawingSessionProvider>();
    }

    [AvaloniaTest]
    public void DrawingSessionViewModel_InitialState()
    {
        new TestScheduler().With(scheduler =>
        {
            RxSchedulers.MainThreadScheduler = scheduler;
            DrawingSession session = new(_initialPicture);
            _ = _mockProvider.Setup(p => p.CurrentSession).Returns(session);

            DrawingSessionViewModel viewModel = new(_mockProvider.Object);

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.CurrentSession, Is.EqualTo(session));
                Assert.That(((System.Windows.Input.ICommand)viewModel.UndoCommand).CanExecute(null), Is.False);
                Assert.That(((System.Windows.Input.ICommand)viewModel.RedoCommand).CanExecute(null), Is.False);
            });
        });
    }

    [AvaloniaTest]
    public void DrawingSessionViewModel_UndoRedoExecution()
    {
        new TestScheduler().With(scheduler =>
        {
            RxSchedulers.MainThreadScheduler = scheduler;
            DrawingSession session = new(_initialPicture);
            Picture nextPicture = Picture.CreateEmpty(_size);
            DrawingSession session2 = session.Push(nextPicture);

            _ = _mockProvider.Setup(p => p.CurrentSession).Returns(session2);
            DrawingSessionViewModel viewModel = new(_mockProvider.Object);
            scheduler.AdvanceBy(1);

            Assert.That(((System.Windows.Input.ICommand)viewModel.UndoCommand).CanExecute(null), Is.True);

            // Undo実行時に Provider.Update が呼ばれることを確認
            _ = viewModel.UndoCommand.Execute().Subscribe();
            scheduler.AdvanceBy(1);

            _mockProvider.Verify(p => p.Update(It.Is<DrawingSession>(s => s.CurrentPicture == _initialPicture)), Times.Once);
        });
    }
}
