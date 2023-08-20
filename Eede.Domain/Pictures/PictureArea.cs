using Eede.Domain.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Domain.Pictures
{
    public class PictureArea
    {
        public readonly Position Position;
        public readonly PictureSize Size;

        public PictureArea(Position position, PictureSize size)
        {
            Position = position;
            Size = size;
        }

        public int X => Position.X;
        public int Y => Position.Y;
        public int Width => Size.Width;
        public int Height => Size.Height;
    }
}
