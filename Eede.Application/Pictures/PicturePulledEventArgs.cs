using Eede.Domain.Pictures;
using Eede.Domain.Positions;
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
            return Picture.CutOut(new PictureArea(new Position(Rect.X, Rect.Y), new PictureSize(Rect.Width, Rect.Height)));
        }
    }
}