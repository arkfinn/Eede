using Eede.Domain.Files;
using Eede.Domain.Pictures;
using System;

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

        public void Write(PrimaryPicture picture)
        {
            if (picture == null)
            {
                throw new ArgumentNullException("picture is null.");
            }
            picture.ToImage().Save(Path.Path, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}