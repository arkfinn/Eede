using Eede.Positions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede
{
    public class ColorDropper
    {
        private readonly Bitmap bitmap;

        public ColorDropper(Bitmap bitmap)
        {
            this.bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
        }

        public Color Drop(Position pos)
        {
            if (!pos.IsInnerOf(bitmap.Size))
            {
                throw new ArgumentOutOfRangeException();
            }
            return bitmap.GetPixel(pos.X, pos.Y);
        }
    }
}
