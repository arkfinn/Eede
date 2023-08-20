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
            using var image = new Bitmap(Path.Path);
            using var tmp = To32bppArgb(image);
            return BitmapConverter.ConvertBack(tmp);
        }

        private static Bitmap To32bppArgb(Bitmap image)
        {
            return image.Clone(new Rectangle(0, 0, image.Width, image.Height), PixelFormat.Format32bppArgb);
        }
    }
}