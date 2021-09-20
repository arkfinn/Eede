using Eede.Positions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede
{
    public class HalfBoxPosition
    {
        private readonly Size BoxSize;
        public readonly Position Position;
        public readonly Position RealPosition;

        public HalfBoxPosition(Size boxSize, Point localPosition)
        {
            BoxSize = boxSize;
            var halfW = BoxSize.Width / 2;
            var halfH = BoxSize.Height / 2;
            Position = new Position(
                localPosition.X / halfW, 
                localPosition.Y / halfH);
            RealPosition = new Position(
                Position.X * halfW,
                Position.Y * halfH
            );
        }

        public Rectangle CreateRealRectangle(Size size)
        {
            return new Rectangle(RealPosition.ToPoint(), size);
        }
   
    }
}
