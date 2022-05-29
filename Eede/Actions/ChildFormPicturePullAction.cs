using Eede.Domain.Pictures;
using Eede.Domain.Systems;
using Eede.Ui;
using System;

namespace Eede.Actions
{
    internal class ChildFormPicturePullAction : IUndoItem
    {
        private readonly DrawableBox ParentBox;
        private readonly Picture NowPicture;
        private readonly Picture PulledPicture;

        public ChildFormPicturePullAction(DrawableBox parentBox, Picture pulledImage)
        {
            ParentBox = parentBox ?? throw new ArgumentNullException(nameof(parentBox));
            NowPicture = new Picture(ParentBox.GetImage());
            PulledPicture = (pulledImage ?? throw new ArgumentNullException(nameof(pulledImage))).Clone();
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
            NowPicture.Dispose();
            PulledPicture.Dispose();
        }
    }
}