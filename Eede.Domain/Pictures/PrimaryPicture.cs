using System;
using System.Drawing;

namespace Eede.Domain.Pictures
{
    public class PrimaryPicture : IDisposable
    {
        public PrimaryPicture(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("imageの値は必ず指定してください。");
            }
            Buffer = new Bitmap(image);
        }

        public Bitmap Buffer { get; private set; }

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