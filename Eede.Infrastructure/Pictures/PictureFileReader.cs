using Eede.Application.Pictures;
using Eede.Domain.Files;
using Eede.Domain.Pictures;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Eede.Infrastructure.Pictures
{
    public class PictureFileReader : IPictureReader
    {
        private readonly FilePath Path;

        public PictureFileReader(FilePath path)
        {
            Path = path ?? throw new ArgumentNullException("path is null.");
            if (Path.IsEmpty())
            {
                throw new InvalidOperationException("読み込み先のFilePathが指定されていません");
            }
        }

        public Picture Read()
        {
            using var image = Image.FromFile(Path.Path);
            using var tmp = To32bppArg(image);
            return new Picture(tmp);
        }

        private static Bitmap To32bppArg(Image image)
        {
            var bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.PageUnit = GraphicsUnit.Pixel;
                g.DrawImageUnscaled(image, 0, 0);
            }
            return bmp;
        }
    }
}