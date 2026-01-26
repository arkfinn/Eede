using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;

namespace Eede.Presentation.ViewModels.Pages
{
    public class DrawingSessionViewModel : ViewModelBase
    {
        private readonly IDrawingSessionProvider _provider;

        [Reactive] public DrawingSession CurrentSession { get; private set; }

        public ReactiveCommand<Unit, Unit> UndoCommand { get; }
        public ReactiveCommand<Unit, Unit> RedoCommand { get; }

        public DrawingSessionViewModel(IDrawingSessionProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            CurrentSession = _provider.CurrentSession;

            _provider.SessionChanged += (newSession) =>
            {
                CurrentSession = newSession;
            };

            UndoCommand = ReactiveCommand.Create(ExecuteUndo, this.WhenAnyValue(
                x => x.CurrentSession,
                (session) => session.CanUndo()));

            RedoCommand = ReactiveCommand.Create(ExecuteRedo, this.WhenAnyValue(
                x => x.CurrentSession,
                (session) => session.CanRedo()));
        }

        private void ExecuteUndo()
        {
            _provider.Update(CurrentSession.Undo());
        }

        private void ExecuteRedo()
        {
            _provider.Update(CurrentSession.Redo());
        }

        public void Push(Picture picture, PictureArea? selectingArea = null, PictureArea? previousArea = null)
        {
            _provider.Update(CurrentSession.Push(picture, selectingArea, previousArea));
        }
    }
}
