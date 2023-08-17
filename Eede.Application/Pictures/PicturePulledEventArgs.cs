using Eede.Domain.Pictures;
using System;
using System.Drawing;

namespace Eede.Application.Pictures
{
    public class PicturePulledEventArgs : EventArgs
    {
        public PicturePulledEventArgs(Picture graphics, Rectangle rect)
        {
            Picture = graphics ?? throw new ArgumentNullException("graphics needs not null.");
            Rect = rect;
        }

        public readonly Picture Picture;

        public readonly Rectangle Rect;

        public Picture CutOutImage()
        {
            return Picture.CutOut(Rect);
        }
    }
}