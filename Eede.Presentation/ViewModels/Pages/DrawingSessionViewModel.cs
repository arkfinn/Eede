using Eede.Domain.ImageEditing;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;

namespace Eede.Presentation.ViewModels.Pages
{
    public class DrawingSessionViewModel : ViewModelBase
    {
        [Reactive] public DrawingSession CurrentSession { get; private set; }

        public ReactiveCommand<Unit, Unit> UndoCommand { get; }
        public ReactiveCommand<Unit, Unit> RedoCommand { get; }

        public DrawingSessionViewModel(DrawingSession initialSession)
        {
            CurrentSession = initialSession ?? throw new ArgumentNullException(nameof(initialSession));

            UndoCommand = ReactiveCommand.Create(ExecuteUndo, this.WhenAnyValue(
                x => x.CurrentSession,
                (session) => session.CanUndo()));

            RedoCommand = ReactiveCommand.Create(ExecuteRedo, this.WhenAnyValue(
                x => x.CurrentSession,
                (session) => session.CanRedo()));
        }

        private void ExecuteUndo()
        {
            CurrentSession = CurrentSession.Undo();
        }

        private void ExecuteRedo()
        {
            CurrentSession = CurrentSession.Redo();
        }

        public void Push(Picture picture)
        {
            CurrentSession = CurrentSession.Push(picture);
        }
    }
}
