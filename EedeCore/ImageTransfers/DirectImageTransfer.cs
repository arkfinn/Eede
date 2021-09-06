using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.ImageTransfers
{
    public class DirectImageTransfer : IImageTransfer
    {
        public void Transfer(Bitmap from, Graphics to, Size size)
        {
            to.PixelOffsetMode = PixelOffsetMode.Half;
            to.InterpolationMode = InterpolationMode.NearestNeighbor;
            to.DrawImage(from, 0, 0, size.Width, size.Height);
        }
    }
}
