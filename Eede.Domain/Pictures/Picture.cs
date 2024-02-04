using Eede.Domain.Colors;
using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
using System;

namespace Eede.Domain.Pictures
{
    public class Picture
    {
        private const int COLOR_32BIT = 4;


        public static Picture Create(PictureSize size, byte[] imageData)
        {
            int stride = size.Width * COLOR_32BIT;
            return stride * size.Height != imageData.Length
                ? throw new ArgumentException($"(width:{size.Width}, height:{size.Height}) * {COLOR_32BIT} != length:{imageData.Length}")
                : new Picture(size, imageData, stride);
        }

        public static Picture CreateEmpty(PictureSize size)
        {
            return Create(size, new byte[size.Width * COLOR_32BIT * size.Height]);
        }

        public readonly PictureSize Size;
        private readonly byte[] ImageData;
        public readonly int Stride;

        private Picture(PictureSize size, byte[] imageData, int stride)
        {
            Size = size;
            ImageData = imageData;
            Stride = stride;
        }

        public int Width => Size.Width;
        public int Height => Size.Height;
        public int Length => ImageData.Length;

        public byte this[int i] => ImageData[i];

        public byte[] CloneImage()
        {
            return ImageData.Clone() as byte[];
        }

        public ArgbColor PickColor(Position pos)
        {
            if (!Contains(pos))
            {
                throw new ArgumentOutOfRangeException();
            }

            int index = (pos.X * COLOR_32BIT) + (Stride * pos.Y);
            return new ArgbColor(
                this[index + 3],
                this[index + 2],
                this[index + 1],
                this[index]);
        }

        public Picture CutOut(PictureArea area)
        {
            int destinationStride = area.Width * COLOR_32BIT;
            int length = Math.Min((Width - area.X) * COLOR_32BIT, destinationStride);
            int destinationX = area.X * COLOR_32BIT;
            byte[] cutImageData = new byte[destinationStride * area.Height];

            for (int i = 0; i < area.Height; i++)
            {
                if (area.Y + i >= Height) break;
                int sourceStartIndex = destinationX + ((area.Y + i) * Stride);
                int destinationStartIndex = i * destinationStride;
                Array.Copy(ImageData, sourceStartIndex, cutImageData, destinationStartIndex, length);
            }

            return Create(area.Size, cutImageData);
        }

        public Picture Transfer(IImageTransfer transfer)
        {
            return Transfer(transfer, new Magnification(1));
        }

        public Picture Transfer(IImageTransfer transfer, Magnification magnification)
        {
            return transfer.Transfer(this, magnification);
        }

        public Picture Blend(IImageBlender blender, Picture src, Position toPosition)
        {
            return blender.Blend(src, this, toPosition);
        }

        public Picture Draw(Func<Picture, Picture> function, IImageBlender blender)
        {
            Picture data = function(this);
            return blender.Blend(data, this);
        }

        public bool Contains(Position position)
        {
            return Size.Contains(position);
        }
    }
}