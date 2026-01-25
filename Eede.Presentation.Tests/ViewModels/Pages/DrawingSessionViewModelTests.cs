using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.ViewModels.Pages;
using NUnit.Framework;
using Avalonia.Headless.NUnit;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using ReactiveUI;
using System.Reactive;

namespace Eede.Presentation.Tests.ViewModels.Pages;

public class DrawingSessionViewModelTests
{
    private Picture _initialPicture;
    private PictureSize _size = new(32, 32);

    [SetUp]
    public void Setup()
    {
        _initialPicture = Picture.CreateEmpty(_size);
    }

    [AvaloniaTest]
    public void DrawingSessionViewModel_InitialState()
    {
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var session = new DrawingSession(_initialPicture);
            var viewModel = new DrawingSessionViewModel(session);

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
            
            var viewModel = new DrawingSessionViewModel(session2);
            scheduler.AdvanceBy(1);

            Assert.That(((System.Windows.Input.ICommand)viewModel.UndoCommand).CanExecute(null), Is.True);
            
            viewModel.UndoCommand.Execute().Subscribe();
            scheduler.AdvanceBy(1);

            Assert.That(viewModel.CurrentSession.CurrentPicture, Is.EqualTo(_initialPicture));
            Assert.That(((System.Windows.Input.ICommand)viewModel.UndoCommand).CanExecute(null), Is.False);
            Assert.That(((System.Windows.Input.ICommand)viewModel.RedoCommand).CanExecute(null), Is.True);
        });
    }
}
