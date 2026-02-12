#nullable enable
using System;
using System.Collections.Immutable;

namespace Eede.Domain.ImageEditing
{
    public sealed record UndoSystem : IDisposable
    {
        public UndoSystem() : this([], [])
        {
        }

        private UndoSystem(ImmutableStack<IUndoItem> undoList, ImmutableStack<IUndoItem> redoList)
        {
            UndoList = undoList;
            RedoList = redoList;
        }

        private readonly ImmutableStack<IUndoItem> UndoList;
        private readonly ImmutableStack<IUndoItem> RedoList;

        public UndoSystem Undo()
        {
            IUndoItem item = UndoList.Peek();
            item.Undo();
            return new UndoSystem(UndoList.Pop(), RedoList.Push(item));
        }

        public UndoSystem Redo()
        {
            IUndoItem item = RedoList.Peek();
            item.Redo();
            return new UndoSystem(UndoList.Push(item), RedoList.Pop());
        }

        public UndoSystem Add(IUndoItem item)
        {
            DisposeRedoList();
            return new UndoSystem(UndoList.Push(item), []);
        }

        public bool CanUndo()
        {
            return !UndoList.IsEmpty;
        }

        public bool CanRedo()
        {
            return !RedoList.IsEmpty;
        }

        public void Dispose()
        {
            DisposeUndoList();
            DisposeRedoList();
        }

        private void DisposeUndoList()
        {
            foreach (IUndoItem item in UndoList)
            {
                item.Dispose();
            }
        }

        private void DisposeRedoList()
        {
            foreach (IUndoItem item in RedoList)
            {
                item.Dispose();
            }
        }
    }
}
