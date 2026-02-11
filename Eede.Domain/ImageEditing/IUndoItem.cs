#nullable enable
using System;

namespace Eede.Domain.ImageEditing
{
    public interface IUndoItem : IDisposable
    {
        void Undo();

        void Redo();
    }
}