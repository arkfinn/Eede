using Eede.Domain.ImageEditing;
using Eede.Domain.Pictures;
using System;

namespace Eede.Presentation.Actions
{
    internal class PullPictureAction : IUndoItem
    {
        private readonly Action<Picture> UpdatePicture;
        private readonly Picture NowPicture;
        private readonly Picture PulledPicture;

        public PullPictureAction(Action<Picture> updatePicture, Picture nowPicture, Picture pulledImage)
        {
            UpdatePicture = updatePicture ?? throw new ArgumentNullException(nameof(updatePicture));
            NowPicture = nowPicture ?? throw new ArgumentNullException(nameof(nowPicture));
            PulledPicture = pulledImage ?? throw new ArgumentNullException(nameof(pulledImage));
        }

        public void Do()
        {
            UpdatePicture(PulledPicture);
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            UpdatePicture(NowPicture);
        }

        public void Dispose()
        {
        }
    }
}