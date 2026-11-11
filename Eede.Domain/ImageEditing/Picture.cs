#nullable enable
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using System;
using System.Runtime.InteropServices;

namespace Eede.Domain.ImageEditing;

public record Picture
{
    private const int COLOR_32BIT = 4;
    private const int PixelSizeInBytes = COLOR_32BIT;

    public static Picture Create(PictureSize size, byte[] imageData)
    {
        if (imageData == null) throw new ArgumentNullException(nameof(imageData));
        int totalBytes = size.Width * COLOR_32BIT * size.Height;
        if (totalBytes != imageData.Length)
        {
            throw new ArgumentException($"(width:{size.Width}, height:{size.Height}) * {COLOR_32BIT} != length:{imageData.Length}");
        }
        int stride = size.Width * COLOR_32BIT;
        return new Picture(size, imageData, stride);
    }

    public static Picture CreateEmpty(PictureSize size)
    {
        int totalBytes = size.Width * COLOR_32BIT * size.Height;
        return Create(size, new byte[totalBytes]);
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

        PictureArea bounds = new(new Position(0, 0), Size);
        PictureArea intersection = bounds.Intersect(area);

        if (!intersection.IsEmpty)
        {
            int copyBytesPerLine = intersection.Width * PixelSizeInBytes;
            ReadOnlySpan<byte> sourceSpan = AsSpan();
            Span<byte> destinationSpan = new(cutoutImageData);

            for (int y = 0; y < intersection.Height; y++)
            {
                int currentSourceY = intersection.Y + y;
                int currentDestY = (intersection.Y - area.Y) + y;

                int sourceStartIndex = (intersection.X * PixelSizeInBytes) + (currentSourceY * Stride);
                int destinationStartIndex = ((intersection.X - area.X) * PixelSizeInBytes) + (currentDestY * destinationStride);

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

    public Picture ReplaceColor(ArgbColor targetColor, ArgbColor replacementColor, PictureArea? area = null)
    {
        if (targetColor.EqualsArgb(replacementColor) || ImageData.Length == 0)
        {
            return this;
        }

        PictureArea bounds = new(new Position(0, 0), Size);
        PictureArea targetArea = area.HasValue ? bounds.Intersect(area.Value) : bounds;
        if (targetArea.IsEmpty)
        {
            return this;
        }

        Span<byte> colorBuf = stackalloc byte[8] { targetColor.Blue, targetColor.Green, targetColor.Red, targetColor.Alpha, replacementColor.Blue, replacementColor.Green, replacementColor.Red, replacementColor.Alpha };
        uint targetUint = MemoryMarshal.Read<uint>(colorBuf[..4]);
        uint replacementUint = MemoryMarshal.Read<uint>(colorBuf[4..]);

        byte[]? clonedPixels = null;
        Span<uint> destUints = default;
        ReadOnlySpan<uint> srcUints = MemoryMarshal.Cast<byte, uint>(AsSpan());

        int startX = targetArea.X;
        int endY = targetArea.Y + targetArea.Height;
        int width = targetArea.Width;

        for (int y = targetArea.Y; y < endY; y++)
        {
            int rowOffset = y * Width + startX;
            ReadOnlySpan<uint> srcRow = srcUints.Slice(rowOffset, width);
            for (int x = 0; x < width; x++)
            {
                if (srcRow[x] == targetUint)
                {
                    if (clonedPixels == null)
                    {
                        clonedPixels = CloneImage();
                        destUints = MemoryMarshal.Cast<byte, uint>(clonedPixels.AsSpan());
                    }
                    destUints[rowOffset + x] = replacementUint;
                }
            }
        }

        return clonedPixels != null ? Create(Size, clonedPixels) : this;
    }
}
