using Eede.Domain.ImageBlenders;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.Systems;
using Eede.Ui;
using System;

namespace Eede.Actions
{
    internal class PushPictureAction : IUndoItem
    {
        private readonly PictureWindow ChildBox;
        private readonly Picture NowPicture;
        private readonly Picture PushedPicture;
        private readonly IImageBlender Blender;
        private readonly Position Position;

        public PushPictureAction(PictureWindow childBox, Picture nowPicture, Picture pushedPicture, IImageBlender blender, Position position)
        {
            ChildBox = childBox ?? throw new ArgumentNullException(nameof(childBox));
            NowPicture = nowPicture ?? throw new ArgumentNullException(nameof(nowPicture));
            PushedPicture = pushedPicture ?? throw new ArgumentNullException(nameof(pushedPicture));
            Blender = blender ?? throw new ArgumentNullException(nameof(blender));
            Position = position ?? throw new ArgumentNullException(nameof(position));
        }

        public void Do()
        {
            ChildBox.SetupPictureBuffer(NowPicture.Blend(Blender, PushedPicture, Position));
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            ChildBox.SetupPictureBuffer(NowPicture);
        }

        public void Dispose()
        {
        }
    }
}