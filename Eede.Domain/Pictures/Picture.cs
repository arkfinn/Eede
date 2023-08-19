using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
using Eede.Domain.Sizes;
using System;
using System.Drawing;

namespace Eede.Domain.Pictures
{
    public class Picture
    {

        public Picture(Size size)
        {
            BufferData = PictureData.CreateEmpty(new PictureSize(size.Width, size.Height));
        }

        public Picture(PictureData data)
        {
            BufferData = data;
        }

        private PictureData BufferData;

        public Picture CutOut(Rectangle rect)
        {
            return new Picture(BufferData.CutOut(rect.X, rect.Y, rect.Width, rect.Height));
        }

        public PictureSize Size => BufferData.Size;
        public int Width => BufferData.Width;
        public int Height => BufferData.Height;
        public int Stride => BufferData.Stride;

        public byte[] CloneImage() => BufferData.CloneImage();

        public PictureData Transfer(IImageTransfer transfer)
        {
            return Transfer(transfer, new Magnification(1));
        }

        public PictureData Transfer(IImageTransfer transfer, Magnification magnification)
        {
            return transfer.Transfer(BufferData, magnification);
        }

        public Picture Blend(IImageBlender blender, Picture src, Position toPosition)
        {
            return new Picture(blender.Blend(src.BufferData, BufferData, toPosition));
        }

        public Picture Draw(Func<PictureData, PictureData> function, IImageBlender blender)
        {
            var data = function(BufferData);
            return new Picture(blender.Blend(data, BufferData));
        }

        public ArgbColor PickColor(Position pos)
        {
            if (!Contains(pos))
            {
                throw new ArgumentOutOfRangeException();
            }
            return BufferData.PickColor(pos);
        }

        public bool Contains(Position position)
        {
            return BufferData.Size.Contains(position);
        }
    }
}