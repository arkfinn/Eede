using Eede.Domain.Pictures;
using System;

namespace Eede.Application.Pictures
{
    public class PicturePushEventArgs : EventArgs
    {
        public PicturePushEventArgs(Picture graphics, PictureArea rect)
        {
            Picture = graphics ?? throw new ArgumentNullException(nameof(graphics));
            Rect = rect;
        }

        public readonly Picture Picture;

        public readonly PictureArea Rect;

        public Picture CutOutImage()
        {
            return Picture.CutOut(Rect);
        }
    }
}