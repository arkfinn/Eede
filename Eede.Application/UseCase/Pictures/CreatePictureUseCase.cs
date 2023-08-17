using Eede.Application.Pictures;
using Eede.Domain.Pictures;
using System.Drawing.Imaging;
using System.Drawing;
using Eede.Domain.Files;

namespace Eede.Application.UseCase.Pictures
{
    public class CreatePictureUseCase
    {
        public PictureFile Execute(Size size)
        {
            using var bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.White, new Rectangle(new Point(0, 0), size));
            }
            return new PictureFile(new FilePath(""), new Picture(bmp));
        }
    }
}
