#nullable enable
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
        if (imageData == null) throw new ArgumentNullException(nameof(imageData));
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
        return (byte[])ImageData.Clone();
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
        if (area.IsEmpty)
        {
            return Create(new PictureSize(0, 0), Array.Empty<byte>());
        }

        int destinationStride = area.Width * PixelSizeInBytes;
        byte[] cutoutImageData = new byte[destinationStride * area.Height];

        // 画像の有効領域 (0, 0, Width, Height) と area の交差範囲を計算
        int startX = Math.Max(0, area.X);
        int startY = Math.Max(0, area.Y);
        int endX = Math.Min(Width, area.X + area.Width);
        int endY = Math.Min(Height, area.Y + area.Height);

        int copyWidth = Math.Max(0, endX - startX);
        int copyHeight = Math.Max(0, endY - startY);

        if (copyWidth > 0 && copyHeight > 0)
        {
            int copyBytesPerLine = copyWidth * PixelSizeInBytes;
            ReadOnlySpan<byte> sourceSpan = AsSpan();
            Span<byte> destinationSpan = new(cutoutImageData);

            for (int y = 0; y < copyHeight; y++)
            {
                int currentSourceY = startY + y;
                int currentDestY = (startY - area.Y) + y;

                int sourceStartIndex = (startX * PixelSizeInBytes) + (currentSourceY * Stride);
                int destinationStartIndex = ((startX - area.X) * PixelSizeInBytes) + (currentDestY * destinationStride);

                sourceSpan.Slice(sourceStartIndex, copyBytesPerLine)
                          .CopyTo(destinationSpan.Slice(destinationStartIndex, copyBytesPerLine));
            }
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

    public Picture Clear(PictureArea area)
    {
        return Blend(new DirectImageBlender(), CreateEmpty(area.Size), area.Position);
    }

    public Picture ApplyTransparency(ArgbColor transparentColor)
    {
        byte[] pixels = CloneImage();
        for (int i = 0; i < pixels.Length; i += 4)
        {
            if (pixels[i] == transparentColor.Blue &&
                pixels[i + 1] == transparentColor.Green &&
                pixels[i + 2] == transparentColor.Red &&
                pixels[i + 3] == transparentColor.Alpha)
            {
                pixels[i + 3] = 0;
            }
        }
        return Create(Size, pixels);
    }
}
