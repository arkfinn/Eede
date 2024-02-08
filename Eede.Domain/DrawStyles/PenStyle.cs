using Eede.Domain.Colors;
using Eede.Domain.ImageBlenders;
using System;

namespace Eede.Domain.DrawStyles
{
    // AlphaOnlyBrushみたいなので実現できるんじゃないの

    public class PenStyle
    {
        public readonly IImageBlender Blender;
        public readonly ArgbColor Color;
        public readonly int Width;

        public PenStyle(IImageBlender blender) : this(blender, new ArgbColor(255, 0, 0, 0), 1)
        {
        }

        public PenStyle(IImageBlender blender, ArgbColor color, int width)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width) + " is 1 or more. then:" + width);
            }

            Blender = blender ?? throw new ArgumentNullException(nameof(blender));
            Color = color;
            Width = width;
        }

        public PenStyle UpdateBlender(IImageBlender blender)
        {
            return new PenStyle(blender, Color, Width);
        }

        public PenStyle UpdateColor(ArgbColor color)
        {
            return new PenStyle(Blender, color, Width);
        }

        public PenStyle UpdateWidth(int width)
        {
            return new PenStyle(Blender, Color, width);
        }
    }
}