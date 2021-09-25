using System;
using System.Drawing;

namespace Eede.Domain.Pictures
{
    public class Picture : IDisposable
    {
        public Picture(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("imageの値は必ず指定してください。");
            }
            Buffer = new Bitmap(image);
        }

        public Bitmap Buffer { get; private set; }

        public Bitmap CutOut(Rectangle rect)
        {
            return Buffer.Clone(rect, Buffer.PixelFormat);
        }

        public Image ToImage()
        {
            return new Bitmap(Buffer);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~Picture()
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