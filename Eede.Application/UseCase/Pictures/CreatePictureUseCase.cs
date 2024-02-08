using Eede.Application.Pictures;
using Eede.Domain.Files;
using System.Drawing;
using System.Drawing.Imaging;

namespace Eede.Application.UseCase.Pictures
{
    public class CreatePictureUseCase
    {
        public PictureFile Execute(Size size)
        {
            using Bitmap bmp = new(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.White, new Rectangle(new Point(0, 0), size));
            }
            return new PictureFile(new FilePath(""), BitmapConverter.ConvertBack(bmp));
        }
    }
}
