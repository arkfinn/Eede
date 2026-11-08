#nullable enable
using System;
using System.Buffers;
using Eede.Application.Palettes;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;

namespace Eede.Infrastructure.Palettes;

/// <summary>
/// 画像のピクセルデータを統合単一パスで走査し、256色以下の完全一致パレットまたは257色以上の代表256色パレットを抽出するスキャナー。
/// </summary>
public static class DirectColorPaletteScanner
{
    private const int HistogramSize = HistogramMedianCutQuantizer.HistogramSize;
    private const int HashTableSize = 512;
    private const uint HashTableMask = HashTableSize - 1;

    /// <summary>
    /// Picture の全ピクセルを単一パスで走査し、256色以下であれば出現順を維持した完全一致パレットを、
    /// 257色以上であればRGB555メディアンカットによる代表256色パレットを生成する。
    /// </summary>
    public static PaletteExtractionResult? Scan(Picture picture)
    {
        ArgumentNullException.ThrowIfNull(picture);

        if (picture.Width <= 0 || picture.Height <= 0)
        {
            return null;
        }

        ReadOnlySpan<byte> pixels = picture.AsSpan();
        int expectedBytes = picture.Width * picture.Height * 4;
        if (pixels.Length < expectedBytes)
        {
            return null;
        }

        int[] rentedHistogram = ArrayPool<int>.Shared.Rent(HistogramSize);
        uint[] rentedAnchorArgb = ArrayPool<uint>.Shared.Rent(HistogramSize);

        try
        {
            Span<int> histogram = rentedHistogram.AsSpan(0, HistogramSize);
            Span<uint> anchorArgb = rentedAnchorArgb.AsSpan(0, HistogramSize);
            histogram.Clear();
            anchorArgb.Clear();

            Span<ArgbColor> uniqueColors = stackalloc ArgbColor[Palette.MAX_LENGTH];
            Span<uint> seenKeys = stackalloc uint[HashTableSize];
            Span<byte> seenOccupied = stackalloc byte[HashTableSize];
            seenOccupied.Clear();

            int uniqueCount = 0;
            bool isExactMatch = true;
            bool hasTransparent = false;

            bool hasPrev = false;
            uint prevPacked = 0;
            int prevColor555 = 0;
            int prevWeight = 0;

            for (int i = 0; i < expectedBytes; i += 4)
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3];

                uint packed = a == 0
                    ? 0u
                    : ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | b;

                if (hasPrev && packed == prevPacked)
                {
                    if (prevWeight > 0)
                    {
                        histogram[prevColor555] += prevWeight;
                    }
                    continue;
                }

                hasPrev = true;
                prevPacked = packed;

                if (a == 0)
                {
                    hasTransparent = true;
                    prevWeight = 0;
                }
                else if (a < 16)
                {
                    prevWeight = 0;
                }
                else
                {
                    int color555 = ((r >> 3) << 10) | ((g >> 3) << 5) | (b >> 3);
                    int weight = a >> 4;
                    histogram[color555] += weight;

                    uint currentAnchor = anchorArgb[color555];
                    if (currentAnchor == 0 || a > (byte)(currentAnchor >> 24))
                    {
                        anchorArgb[color555] = packed;
                    }

                    prevColor555 = color555;
                    prevWeight = weight;
                }

                if (isExactMatch)
                {
                    uint slot = (packed * 2654435761u) >> 23;
                    bool found = false;
                    while (seenOccupied[(int)slot] != 0)
                    {
                        if (seenKeys[(int)slot] == packed)
                        {
                            found = true;
                            break;
                        }
                        slot = (slot + 1) & HashTableMask;
                    }

                    if (!found)
                    {
                        if (uniqueCount >= Palette.MAX_LENGTH)
                        {
                            isExactMatch = false;
                        }
                        else
                        {
                            seenOccupied[(int)slot] = 1;
                            seenKeys[(int)slot] = packed;
                            uniqueColors[uniqueCount++] = a == 0
                                ? new ArgbColor(0, 0, 0, 0)
                                : new ArgbColor(a, r, g, b);
                        }
                    }
                }
            }

            if (isExactMatch)
            {
                ArgbColor[] fullColors = new ArgbColor[Palette.MAX_LENGTH];
                for (int i = 0; i < Palette.MAX_LENGTH; i++)
                {
                    fullColors[i] = i < uniqueCount ? uniqueColors[i] : new ArgbColor(0, 0, 0, 0);
                }
                return new PaletteExtractionResult(Palette.FromColors(fullColors), IsRepresentative: false);
            }

            Palette representativePalette = HistogramMedianCutQuantizer.Quantize(
                histogram,
                anchorArgb,
                uniqueColors,
                hasTransparent);

            return new PaletteExtractionResult(representativePalette, IsRepresentative: true);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(rentedHistogram);
            ArrayPool<uint>.Shared.Return(rentedAnchorArgb);
        }
    }
}
