using Eede.Application.Pictures;
using Eede.Domain.Files;
using Eede.Domain.Pictures;
using System;
using System.Drawing;

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
            using (var image = Image.FromFile(Path.Path))
            {
                return new Picture(new Bitmap(image));
            }
        }
    }
}