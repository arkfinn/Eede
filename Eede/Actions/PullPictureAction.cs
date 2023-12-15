using Eede.Domain.Pictures;
using Eede.Domain.Systems;
using Eede.Ui;
using System;

namespace Eede.Actions
{
    internal class PullPictureAction : IUndoItem
    {
        private readonly DrawableBox ParentBox;
        private readonly Picture NowPicture;
        private readonly Picture PulledPicture;

        public PullPictureAction(DrawableBox parentBox, Picture pulledImage)
        {
            ParentBox = parentBox ?? throw new ArgumentNullException(nameof(parentBox));
            NowPicture = ParentBox.GetImage();
            PulledPicture = pulledImage ?? throw new ArgumentNullException(nameof(pulledImage));
        }

        public void Do()
        {
            ParentBox.SetupPicture(PulledPicture);
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            ParentBox.SetupPicture(NowPicture);
        }

        public void Dispose()
        {
        }
    }
}