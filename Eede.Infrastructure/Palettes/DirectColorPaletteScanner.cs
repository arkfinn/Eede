#nullable enable
using System;
using System.Collections.Generic;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;

namespace Eede.Infrastructure.Palettes;

/// <summary>
/// 画像のピクセルデータからユニーク色を走査し、256色以下のパレットを抽出するスキャナー。
/// </summary>
public static class DirectColorPaletteScanner
{
    /// <summary>
    /// Picture の全ピクセルを走査し、256色以下であれば出現順を維持した Palette を生成する。
    /// ユニーク色が 257 色に達した場合は即座に走査を打ち切って null を返す。
    /// </summary>
    public static Palette? Scan(Picture picture)
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

        HashSet<ArgbColor> seen = [];
        List<ArgbColor> orderedColors = new(Palette.MAX_LENGTH);

        for (int i = 0; i < expectedBytes; i += 4)
        {
            byte b = pixels[i];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];
            byte a = pixels[i + 3];

            // 完全透明ピクセルはRGBに関わらず統一して (0, 0, 0, 0) に正規化
            ArgbColor color = a == 0 ? new ArgbColor(0, 0, 0, 0) : new ArgbColor(a, r, g, b);

            if (seen.Add(color))
            {
                if (orderedColors.Count >= Palette.MAX_LENGTH)
                {
                    // 257色目を検出した瞬間に即座に脱出
                    return null;
                }
                orderedColors.Add(color);
            }
        }

        ArgbColor[] fullColors = new ArgbColor[Palette.MAX_LENGTH];
        for (int i = 0; i < Palette.MAX_LENGTH; i++)
        {
            fullColors[i] = i < orderedColors.Count ? orderedColors[i] : new ArgbColor(0, 0, 0, 0);
        }

        return Palette.FromColors(fullColors);
    }
}
