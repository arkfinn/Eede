using Eede.Domain.Systems;
using System;

namespace Eede.Presentation.Actions
{
    internal class UndoItem : IUndoItem
    {
        private readonly Action UndoAction;
        private readonly Action RedoAction;

        public UndoItem(Action undoAction, Action redoAction)
        {
            UndoAction = undoAction ?? throw new ArgumentNullException(nameof(undoAction));
            RedoAction = redoAction ?? throw new ArgumentNullException(nameof(redoAction));
        }

        public void Dispose()
        {
            // nothing
        }

        public void Redo()
        {
            RedoAction.Invoke();
        }

        public void Undo()
        {
            UndoAction.Invoke();
        }
    }
}
