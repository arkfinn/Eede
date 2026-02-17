using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.History;
using Eede.Domain.SharedKernel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;

namespace Eede.Presentation.ViewModels.Pages
{
#nullable enable

    public class DrawingSessionViewModel : ViewModelBase
    {
        private readonly IDrawingSessionProvider _provider;

        [Reactive] public DrawingSession CurrentSession { get; private set; }

        public ReactiveCommand<Unit, Unit> UndoCommand { get; }
        public ReactiveCommand<Unit, Unit> RedoCommand { get; }

        public event EventHandler<UndoResult>? Undone;
        public event EventHandler<RedoResult>? Redone;

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
            var result = CurrentSession.Undo();
            _provider.Update(result.Session);
            Undone?.Invoke(this, result);
        }

        private void ExecuteRedo()
        {
            var result = CurrentSession.Redo();
            _provider.Update(result.Session);
            Redone?.Invoke(this, result);
        }

        public void Push(Picture picture, PictureArea? selectingArea = null, PictureArea? previousArea = null, PictureRegion affectedArea = default, Picture? beforePicture = null)
        {
            if (!affectedArea.IsEmpty)
            {
                _provider.Update(CurrentSession.PushDiff(picture, affectedArea, selectingArea, previousArea, beforePicture));
            }
            else
            {
                _provider.Update(CurrentSession.Push(picture, selectingArea, previousArea, beforePicture));
            }
        }

        public void PushDockUpdate(string dockId, Position position, Picture before, Picture after, bool beforeEdited, bool afterEdited)
        {
            _provider.Update(CurrentSession.PushDockUpdate(dockId, position, before, after, beforeEdited, afterEdited));
        }

        public void Sync(DrawingSession session)
        {
            _provider.Update(session);
        }
    }
}
