using Eede.Application.Pictures;
using Eede.Domain.Files;
using Eede.Domain.Pictures;
using System;
using System.Drawing.Imaging;

namespace Eede.Infrastructure.Pictures
{
    public class PictureFileWriter : IPictureWriter
    {
        private readonly FilePath Path;

        public PictureFileWriter(FilePath path)
        {
            Path = path ?? throw new ArgumentNullException("path is null.");
            if (Path.IsEmpty())
            {
                throw new InvalidOperationException("保存先のFilePathが指定されていません");
            }
        }

        public void Write(Picture picture)
        {
            if (picture == null)
            {
                throw new ArgumentNullException("picture is null.");
            }
            using (var image = picture.ToImage())
            {
                image.Save(Path.Path, ImageFormat.Png);
            }
        }
    }
}