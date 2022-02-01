using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System;

namespace Eede.Application.Pictures
{
    public class PicturePushedEventArgs : EventArgs
    {
        public PicturePushedEventArgs(Picture graphics, Position position)
        {
            Picture = graphics ?? throw new ArgumentNullException("graphics needs not null.");
            Position = position ?? throw new ArgumentNullException("position needs not null.");
        }

        public readonly Picture Picture;
        public readonly Position Position;
    }
}