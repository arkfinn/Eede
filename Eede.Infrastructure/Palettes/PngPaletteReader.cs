#nullable enable
using System;
using System.Buffers.Binary;
using System.IO;
using Eede.Domain.Palettes;

namespace Eede.Infrastructure.Palettes;

/// <summary>
/// PNG バイナリから PLTE および tRNS チャンクを読み取り、インデックスカラーパレットを抽出するリーダー。
/// </summary>
public static class PngPaletteReader
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static Palette? Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        long originalPosition = stream.CanSeek ? stream.Position : 0;
        try
        {
            return ReadInternal(stream);
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }

    private static Palette? ReadInternal(Stream stream)
    {
        Span<byte> header = stackalloc byte[8];
        if (!TryReadExactly(stream, header) || !header.SequenceEqual(PngSignature))
        {
            return null;
        }

        bool isIndexed = false;
        ArgbColor[]? paletteEntries = null;

        Span<byte> chunkHeader = stackalloc byte[8];
        Span<byte> crcBuffer = stackalloc byte[4];

        while (TryReadExactly(stream, chunkHeader))
        {
            uint length = BinaryPrimitives.ReadUInt32BigEndian(chunkHeader[..4]);
            uint type = BinaryPrimitives.ReadUInt32BigEndian(chunkHeader[4..8]);

            // チャンクタイプ文字コード
            // "IHDR" = 0x49484452
            // "PLTE" = 0x504C5445
            // "tRNS" = 0x74524E53
            // "IDAT" = 0x49444154
            // "IEND" = 0x49454E44

            if (type == 0x49484452) // IHDR
            {
                if (length < 13) return null;
                byte[] ihdrData = new byte[length];
                if (!TryReadExactly(stream, ihdrData)) return null;

                byte colorType = ihdrData[9];
                if (colorType != 3) // 3 = Indexed-color
                {
                    // インデックスカラーPNGではない
                    return null;
                }
                isIndexed = true;
            }
            else if (type == 0x504C5445) // PLTE
            {
                if (!isIndexed || length > 768 || length % 3 != 0)
                {
                    return null;
                }

                int colorCount = (int)(length / 3);
                byte[] plteData = new byte[length];
                if (!TryReadExactly(stream, plteData)) return null;

                paletteEntries = new ArgbColor[Palette.MAX_LENGTH];
                for (int i = 0; i < colorCount; i++)
                {
                    byte r = plteData[i * 3];
                    byte g = plteData[i * 3 + 1];
                    byte b = plteData[i * 3 + 2];
                    paletteEntries[i] = new ArgbColor(255, r, g, b);
                }
                for (int i = colorCount; i < Palette.MAX_LENGTH; i++)
                {
                    paletteEntries[i] = new ArgbColor(0, 0, 0, 0);
                }
            }
            else if (type == 0x74524E53) // tRNS
            {
                if (isIndexed && paletteEntries != null)
                {
                    byte[] trnsData = new byte[length];
                    if (!TryReadExactly(stream, trnsData)) return null;

                    int count = Math.Min((int)length, Palette.MAX_LENGTH);
                    for (int i = 0; i < count; i++)
                    {
                        var c = paletteEntries[i];
                        paletteEntries[i] = new ArgbColor(trnsData[i], c.Red, c.Green, c.Blue);
                    }
                }
                else
                {
                    SkipBytes(stream, length);
                }
            }
            else if (type == 0x49444154 || type == 0x49454E44) // IDAT or IEND
            {
                // PLTE / tRNS は IDAT より前に配置される仕様のため走査終了
                break;
            }
            else
            {
                SkipBytes(stream, length);
            }

            // CRC 4バイトをスキップ
            if (!TryReadExactly(stream, crcBuffer))
            {
                break;
            }
        }

        return paletteEntries != null ? Palette.FromColors(paletteEntries) : null;
    }

    private static bool TryReadExactly(Stream stream, Span<byte> buffer)
    {
        int totalRead = 0;
        while (totalRead < buffer.Length)
        {
            int read = stream.Read(buffer[totalRead..]);
            if (read == 0) return false;
            totalRead += read;
        }
        return true;
    }

    private static void SkipBytes(Stream stream, long count)
    {
        if (stream.CanSeek)
        {
            stream.Seek(count, SeekOrigin.Current);
        }
        else
        {
            Span<byte> buffer = stackalloc byte[512];
            long remaining = count;
            while (remaining > 0)
            {
                int read = stream.Read(buffer[..(int)Math.Min(buffer.Length, remaining)]);
                if (read == 0) break;
                remaining -= read;
            }
        }
    }
}
