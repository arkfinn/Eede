using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.ViewModels.Pages;
using NUnit.Framework;
using Avalonia.Headless.NUnit;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using ReactiveUI;
using System.Reactive;
using Moq;

namespace Eede.Presentation.Tests.ViewModels.Pages;

public class DrawingSessionViewModelTests
{
    private Picture _initialPicture;
    private PictureSize _size = new(32, 32);
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
            RxApp.MainThreadScheduler = scheduler;
            var session = new DrawingSession(_initialPicture);
            _mockProvider.Setup(p => p.CurrentSession).Returns(session);

            var viewModel = new DrawingSessionViewModel(_mockProvider.Object);

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
            RxApp.MainThreadScheduler = scheduler;
            var session = new DrawingSession(_initialPicture);
            var nextPicture = Picture.CreateEmpty(_size);
            var session2 = session.Push(nextPicture);
            
            _mockProvider.Setup(p => p.CurrentSession).Returns(session2);
            var viewModel = new DrawingSessionViewModel(_mockProvider.Object);
            scheduler.AdvanceBy(1);

            Assert.That(((System.Windows.Input.ICommand)viewModel.UndoCommand).CanExecute(null), Is.True);
            
            // Undo実行時に Provider.Update が呼ばれることを確認
            viewModel.UndoCommand.Execute().Subscribe();
            scheduler.AdvanceBy(1);

            _mockProvider.Verify(p => p.Update(It.Is<DrawingSession>(s => s.CurrentPicture == _initialPicture)), Times.Once);
        });
    }
}
