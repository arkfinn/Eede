#nullable enable
using System;

namespace Eede.Domain.ImageEditing
{
    public interface IUndoItem
    {
        void Undo();

        void Redo();
    }
}