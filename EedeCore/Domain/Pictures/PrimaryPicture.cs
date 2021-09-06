using Eede.Domain.Files;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Domain.Pictures
{
    public class PrimaryPicture
    {
        public PrimaryPicture(FilePath path, Bitmap picture)
        {
            if (picture.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ArgumentException(
                    "AlphaPictureではPixelFormat.Format32bppArgbしか使えません:"
                    + picture.PixelFormat.ToString() + "が設定されています"
                );
            }
            Path = path;
            Buffer = picture;
        }

        private FilePath Path;

        public Bitmap Buffer { get; set; }

        public bool IsEmptyFileName()
        {
            return Path.IsEmpty();
        }

        public PrimaryPicture Rename(FilePath path)
        {
            return new PrimaryPicture(path, Buffer);
        }

        public void Save(IPictureCommandService command)
        {
            if (Path.IsEmpty())
            {
                throw new InvalidOperationException("保存先のFilePathが指定されていません");
            }
            command.Save(Path, Buffer);
        }

    }
}
