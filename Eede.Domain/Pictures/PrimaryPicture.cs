using Eede.Domain.Files;
using System;
using System.Drawing;

namespace Eede.Domain.Pictures
{
    public class PrimaryPicture : IDisposable
    {
        public PrimaryPicture(FilePath path, Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("imageの値は必ず指定してください。");
            }
            Path = path;
            Buffer = new Bitmap(image);
        }

        private FilePath Path;

        public Bitmap Buffer { get; private set; }

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

        public Image ToImage()
        {
            return new Bitmap(Buffer);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~PrimaryPicture()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Buffer.Dispose();
                Buffer = null;
            }
        }
    }
}