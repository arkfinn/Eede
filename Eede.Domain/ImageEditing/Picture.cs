using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.ImageEditing;

public record Picture
{
    private const int COLOR_32BIT = 4;
    private const int PixelSizeInBytes = COLOR_32BIT;

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

    public ReadOnlySpan<byte> AsSpan()
    {
        return new ReadOnlySpan<byte>(ImageData);
    }

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

        int index = pos.X * COLOR_32BIT + Stride * pos.Y;
        ReadOnlySpan<byte> span = AsSpan();
        return new ArgbColor(
            span[index + 3],
            span[index + 2],
            span[index + 1],
            span[index]);
    }

    public Picture CutOut(PictureArea area)
    {
        int destinationStride = area.Width * PixelSizeInBytes;
        int length = Math.Min((Width - area.X) * PixelSizeInBytes, destinationStride);
        int destinationX = area.X * PixelSizeInBytes;
        byte[] cutoutImageData = new byte[destinationStride * area.Height];
        ReadOnlySpan<byte> sourceSpan = AsSpan();
        Span<byte> destinationSpan = new(cutoutImageData);

        for (int y = 0; y < area.Height; y++)
        {
            if (area.Y + y >= Height)
            {
                break;
            }

            int sourceStartIndex = destinationX + (area.Y + y) * Stride;
            int destinationStartIndex = y * destinationStride;
            sourceSpan.Slice(sourceStartIndex, length).CopyTo(destinationSpan.Slice(destinationStartIndex, length));
        }

        return Create(area.Size, cutoutImageData);
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
