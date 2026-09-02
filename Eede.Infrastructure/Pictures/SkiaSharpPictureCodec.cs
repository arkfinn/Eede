#nullable enable
using System;
using System.IO;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using SkiaSharp;

namespace Eede.Infrastructure.Pictures;

public class SkiaSharpPictureCodec : IPictureCodec
{
    private static readonly byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public byte[] EncodeToPng(Picture picture)
    {
        ArgumentNullException.ThrowIfNull(picture);

        if (picture.Width <= 0 || picture.Height <= 0)
        {
            throw new ArgumentException(
                $"Invalid picture dimensions: {picture.Width}x{picture.Height}. Dimensions must be positive.",
                nameof(picture));
        }

        ReadOnlySpan<byte> srcSpan = picture.AsSpan();
        int expectedLength = picture.Width * picture.Height * 4;
        if (srcSpan.Length != expectedLength)
        {
            throw new ArgumentException(
                $"Picture pixel data length mismatch. Expected {expectedLength} bytes, but got {srcSpan.Length} bytes.",
                nameof(picture));
        }

        var info = new SKImageInfo(picture.Width, picture.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        using var bitmap = new SKBitmap(info);
        IntPtr pixelsPtr = bitmap.GetPixels();
        if (pixelsPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to allocate memory for SKBitmap pixels.");
        }

        int srcRowBytes = picture.Width * 4;
        int dstRowBytes = bitmap.RowBytes;

        unsafe
        {
            byte* dst = (byte*)pixelsPtr;
            fixed (byte* src = srcSpan)
            {
                if (dstRowBytes == srcRowBytes)
                {
                    Buffer.MemoryCopy(src, dst, (long)dstRowBytes * picture.Height, srcSpan.Length);
                }
                else
                {
                    for (int y = 0; y < picture.Height; y++)
                    {
                        Buffer.MemoryCopy(src + y * srcRowBytes, dst + y * dstRowBytes, dstRowBytes, srcRowBytes);
                    }
                }
            }
        }

        using var image = SKImage.FromBitmap(bitmap);
        if (image == null)
        {
            throw new InvalidOperationException("Failed to create SKImage from SKBitmap.");
        }

        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        if (data == null)
        {
            throw new InvalidOperationException("Failed to encode image to PNG format.");
        }

        return data.ToArray();
    }

    public Picture DecodeFromPng(byte[] pngBytes)
    {
        ArgumentNullException.ThrowIfNull(pngBytes);

        if (pngBytes.Length == 0)
        {
            throw new ArgumentException("PNG data cannot be empty.", nameof(pngBytes));
        }

        if (pngBytes.Length < PngHeader.Length || !pngBytes.AsSpan(0, PngHeader.Length).SequenceEqual(PngHeader))
        {
            throw new ArgumentException("The provided data does not match the PNG file signature.", nameof(pngBytes));
        }

        using var stream = new MemoryStream(pngBytes);
        using var skStream = new SKManagedStream(stream);
        using var codec = SKCodec.Create(skStream);
        if (codec == null)
        {
            throw new ArgumentException("Failed to create SKCodec. The PNG data may be corrupted.", nameof(pngBytes));
        }

        if (codec.EncodedFormat != SKEncodedImageFormat.Png)
        {
            throw new ArgumentException($"Unsupported image format: {codec.EncodedFormat}. Expected PNG.", nameof(pngBytes));
        }

        int width = codec.Info.Width;
        int height = codec.Info.Height;
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException($"Invalid image dimensions from PNG header: {width}x{height}.", nameof(pngBytes));
        }

        var desiredInfo = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        using var bitmap = new SKBitmap(desiredInfo);
        IntPtr pixelsPtr = bitmap.GetPixels();
        if (pixelsPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to allocate memory for SKBitmap pixels during decoding.");
        }

        var codecResult = codec.GetPixels(desiredInfo, pixelsPtr);
        if (codecResult != SKCodecResult.Success && codecResult != SKCodecResult.IncompleteInput)
        {
            throw new ArgumentException($"Failed to decode PNG image pixels. Result: {codecResult}", nameof(pngBytes));
        }

        byte[] pixels = new byte[width * height * 4];
        int srcRowBytes = bitmap.RowBytes;
        int dstRowBytes = width * 4;

        unsafe
        {
            byte* src = (byte*)pixelsPtr;
            fixed (byte* dst = pixels)
            {
                if (srcRowBytes == dstRowBytes)
                {
                    Buffer.MemoryCopy(src, dst, pixels.Length, (long)srcRowBytes * height);
                }
                else
                {
                    for (int y = 0; y < height; y++)
                    {
                        Buffer.MemoryCopy(src + y * srcRowBytes, dst + y * dstRowBytes, dstRowBytes, dstRowBytes);
                    }
                }
            }
        }

        return Picture.Create(new PictureSize(width, height), pixels);
    }
}
