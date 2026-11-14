#nullable enable
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.Palettes;
using System;

namespace Eede.Domain.ImageEditing.DrawingTools;

// AlphaOnlyBrushみたいなので実現できるんじゃないの

public class PenStyle
{
    public readonly IImageBlender Blender;
    public readonly ArgbColor Color;
    public readonly int Width;
    public readonly TilePattern TilePattern;

    public PenStyle(IImageBlender blender) : this(blender, new ArgbColor(255, 0, 0, 0), 1, TilePattern.Solid)
    {
    }

    public PenStyle(IImageBlender blender, ArgbColor color, int width)
        : this(blender, color, width, TilePattern.Solid)
    {
    }

    public PenStyle(IImageBlender blender, ArgbColor color, int width, TilePattern tilePattern)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width) + " is 1 or more. then:" + width);
        }

        Blender = blender ?? throw new ArgumentNullException(nameof(blender));
        Color = color;
        Width = width;
        TilePattern = tilePattern ?? throw new ArgumentNullException(nameof(tilePattern));
    }

    public PenStyle UpdateBlender(IImageBlender blender)
    {
        return new PenStyle(blender, Color, Width, TilePattern);
    }

    public PenStyle UpdateColor(ArgbColor color)
    {
        return new PenStyle(Blender, color, Width, TilePattern);
    }

    public PenStyle UpdateWidth(int width)
    {
        return new PenStyle(Blender, Color, width, TilePattern);
    }

    public PenStyle UpdateTilePattern(TilePattern tilePattern)
    {
        return new PenStyle(Blender, Color, Width, tilePattern);
    }
}
