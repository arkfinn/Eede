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

        public Picture(Bitmap image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("imageの値は必ず指定してください。");
            }
            Buffer = CreatelCone(image);
        }

        private Bitmap CreatelCone(Bitmap source)
        {
            var tmp = new Bitmap(source.Width, source.Height);
            var d = new DirectImageBlender();
            d.Blend(source, tmp);
            return tmp;
        }

        public Picture Clone()
        {
            return new Picture(Buffer);
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
            Transfer(transfer, g, Buffer.Size);
        }

        public void Transfer(IImageTransfer transfer, Graphics g, Size size)
        {
            transfer.Transfer(Buffer, g, size);
        }

        // TODO: Bufferを書き換えているので副作用があるため、Transfer同様に別画像に対して転送するか、DrawのようにPictureを返したい。
        // IImageBlenderのメソッドが整っていないため一時的にDirectImageBlenderを直で指定
        public void Blend(/*IImageBlender*/ DirectImageBlender blender, Bitmap src, Position toPosition)
        {
            blender.Blend(src, Buffer, toPosition);
        }

        public Picture Draw(Action<Graphics> action, IImageBlender blender)
        {
            using (var newBmp = CreatelCone(Buffer))
            using (var tmp = CreatelCone(Buffer))
            {
                using (var g = Graphics.FromImage(tmp))
                {
                    action(g);
                }
                blender.Blend(tmp, newBmp);
                return new Picture(newBmp);
            }
        }

        public Color PickColor(Position pos)
        {
            if (!Contains(pos))
            {
                throw new ArgumentOutOfRangeException();
            }
            return Buffer.GetPixel(pos.X, pos.Y);
        }

        public bool Contains(Position position)
        {
            return position.IsInnerOf(Buffer.Size);
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