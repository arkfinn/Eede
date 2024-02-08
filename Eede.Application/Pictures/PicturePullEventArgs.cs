using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System;

namespace Eede.Application.Pictures
{
    public class PicturePullEventArgs : EventArgs
    {
        public PicturePullEventArgs(Picture graphics, Position position)
        {
            Picture = graphics ?? throw new ArgumentNullException(nameof(graphics));
            Position = position ?? throw new ArgumentNullException(nameof(position));
        }

        public readonly Picture Picture;
        public readonly Position Position;
    }
}