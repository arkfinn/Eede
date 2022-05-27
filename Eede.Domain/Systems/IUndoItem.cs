using System;

namespace Eede.Domain.Systems
{
    public interface IUndoItem : IDisposable
    {
        void Undo();

        void Redo();
    }
}