using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
using Eede.Domain.Sizes;
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
            Buffer = CreateClone(image);
        }

        public Picture(Size size)
        {
            Buffer = new Bitmap(size.Width, size.Height);
        }

        public Picture(PictureData data)
        {
            Buffer = PictureData.CreateBitmap(data);
        }

        private Bitmap CreateClone(Bitmap source)
        {
            // new Bitmap(source)とすると、alpha=0のRGB情報が失われるため、手動で合成し直している。
            using var tmp = new Bitmap(source.Width, source.Height);
            var d = new DirectImageBlender();
            var src = PictureData.CreateBuffer(source);
            return PictureData.CreateBitmap(d.Blend(src, PictureData.CreateBuffer(tmp)));
        }

        public Picture Clone()
        {
            return new Picture(Buffer);
        }

        private Bitmap buffer;
        private Bitmap Buffer
        {
            get { return buffer; }
            set
            {
                buffer = value;
                BufferData = PictureData.CreateBuffer(buffer);
            }
        }
        private PictureData BufferData;

        public Bitmap CutOut(Rectangle rect)
        {
            return Buffer.Clone(rect, Buffer.PixelFormat);
        }

        public Image ToImage()
        {
            return PictureData.CreateBitmap(BufferData);
        }

        public Size Size => Buffer.Size;

        public PictureData Transfer(IImageTransfer transfer)
        {
            return Transfer(transfer, new Magnification(1));
        }

        public PictureData Transfer(IImageTransfer transfer, Magnification magnification)
        {
            return transfer.Transfer(BufferData, magnification);
        }

        /// <summary>
        /// このPictureのtoPositionにsrcを合成した新しいPictureを返す。
        /// </summary>
        /// <param name="blender"></param>
        /// <param name="src"></param>
        /// <param name="toPosition"></param>
        /// <returns></returns>
        //public Picture Blend(IImageBlender blender, Bitmap src, Position toPosition)
        //{
        //    using (var tmp = CreateClone(Buffer))
        //    {
        //        blender.Blend(src, tmp, toPosition);
        //        return new Picture(tmp);
        //    }
        //}

        public Picture Blend(IImageBlender blender, Picture src, Position toPosition)
        {
            return new Picture(blender.Blend(src.BufferData, BufferData, toPosition));
        }

        public Picture Draw(Func<PictureData, PictureData> function, IImageBlender blender)
        {
            var data = function(BufferData);
            return new Picture(blender.Blend(data, BufferData));
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

        protected virtual void Dispose(bool disposing)
        {
            Buffer.Dispose();
        }
    }
}