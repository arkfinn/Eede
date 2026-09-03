#nullable enable
using System;
using System.IO;
using Eede.Domain.Palettes;
using Eede.Infrastructure.ImageEditing.Persistence.ArvFileFormat;

namespace Eede.Infrastructure.Palettes;

/// <summary>
/// ARV ファイルから 16 色のカラーパレットを抽出するリーダー。
/// </summary>
public static class ArvPaletteReader
{
    private const int ArvColorPlanes = 4;

    public static Palette? Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        long originalPosition = stream.CanSeek ? stream.Position : 0;
        try
        {
            using BinaryReader reader = new(stream, System.Text.Encoding.Default, leaveOpen: true);

            // 1. ヘッダーフラグの読み取り (24バイト)
            byte[] dummy16 = reader.ReadBytes(16);
            if (dummy16.Length < 16) return null;

            byte[] arvHeaderFlags = reader.ReadBytes(6);
            if (arvHeaderFlags.Length < 6) return null;

            byte[] dummy2 = reader.ReadBytes(2);
            if (dummy2.Length < 2) return null;

            // 2. 寸法 (4バイト) + 予約領域 (12バイト)
            _ = reader.ReadUInt16(); // width
            _ = reader.ReadUInt16(); // height
            byte[] dummy12 = reader.ReadBytes(12);
            if (dummy12.Length < 12) return null;

            // 3. 画像データのスキップ
            if (arvHeaderFlags[0] == 'I')
            {
                int imageDataLength = reader.ReadUInt16();
                if (imageDataLength < 2) return null;
                reader.ReadBytes(imageDataLength - 2);
            }

            // 4. パレットフラグの検証
            if (arvHeaderFlags[1] != 'R')
            {
                return null;
            }

            // 5. パレットデータの読み取り
            byte paletteLength = reader.ReadByte();
            _ = reader.ReadByte(); // skip 1 byte
            byte[] paletteBytes = reader.ReadBytes(paletteLength - 2);

            ArgbColor[] fullColors = new ArgbColor[Palette.MAX_LENGTH];
            int colorCount = 1 << ArvColorPlanes; // 16色

            for (int colorIndex = 0; colorIndex < colorCount; colorIndex++)
            {
                int offset = colorIndex * 6;
                if (offset + 5 >= paletteBytes.Length)
                {
                    return null;
                }

                byte redByte = paletteBytes[offset];
                byte greenByte = paletteBytes[offset + 2];
                byte blueByte = paletteBytes[offset + 4];

                fullColors[colorIndex] = new ArgbColor(255,
                    (byte)(redByte * 17),
                    (byte)(greenByte * 17),
                    (byte)(blueByte * 17));
            }

            for (int i = colorCount; i < Palette.MAX_LENGTH; i++)
            {
                fullColors[i] = new ArgbColor(0, 0, 0, 0);
            }

            return Palette.FromColors(fullColors);
        }
        catch (Exception ex) when (ex is IOException or EndOfStreamException)
        {
            return null;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }
}
