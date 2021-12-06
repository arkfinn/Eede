using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Positions;
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

        private Bitmap Buffer;

        public Bitmap CutOut(Rectangle rect)
        {
            return Buffer.Clone(rect, Buffer.PixelFormat);
        }

        public Image ToImage()
        {
            return new Bitmap(Buffer);
        }

        public Size Size => Buffer.Size;

        public void Transfer(IImageTransfer transfer, Graphics g)
        {
            transfer.Transfer(Buffer, g, Buffer.Size);
        }

        // IImageBlenderのメソッドが整っていないため一時的にDirectImageBlenderを直で指定
        public void Blend(/*IImageBlender*/ DirectImageBlender blender, Bitmap src, Position toPosition)
        {
            blender.Blend(src, Buffer, toPosition);
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
            if (Buffer != null)
            {
                Buffer.Dispose();
                Buffer = null;
            }
        }
    }
}