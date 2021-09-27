using Eede.Domain.Positions;
using System;
using System.Drawing;

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