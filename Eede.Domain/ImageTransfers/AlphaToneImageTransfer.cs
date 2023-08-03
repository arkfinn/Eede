using Eede.Domain.Sizes;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Eede.Domain.ImageTransfers
{
    public class AlphaToneImageTransfer : IImageTransfer
    {
        public void Transfer(Bitmap from, Graphics to, MagnifiedSize size)
        {
            var ia = new ImageAttributes();
            ia.SetColorMatrix(new ColorMatrix(new float[][]{
                new float[]{ 0.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                new float[]{ 0.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                new float[]{ 0.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                new float[]{ 1.0f, 1.0f, 1.0f, 1.0f, 0.0f},
                new float[]{ 0.0f, 0.0f, 0.0f, 255.0f, 1.0f}
            }));

            to.PixelOffsetMode = PixelOffsetMode.Half;
            to.InterpolationMode = InterpolationMode.NearestNeighbor;
            to.DrawImage(from,
                new Rectangle(0, 0, size.Width, size.Height),
                0, 0, from.Width, from.Height, GraphicsUnit.Pixel, ia);
        }
    }
}