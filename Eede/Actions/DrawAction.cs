using Eede.Domain.Pictures;
using Eede.Domain.Systems;
using Eede.Ui;
using System;

namespace Eede.Actions
{
    internal class DrawAction : IUndoItem
    {
        private readonly DrawableBox ParentBox;
        private readonly Picture NowPicture;
        private readonly Picture PulledPicture;

        public DrawAction(DrawableBox parentBox, Picture nowPicture, Picture drewPicture)
        {
            ParentBox = parentBox ?? throw new ArgumentNullException(nameof(parentBox));
            NowPicture = nowPicture ?? throw new ArgumentNullException(nameof(nowPicture));
            PulledPicture = drewPicture ?? throw new ArgumentNullException(nameof(drewPicture));
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