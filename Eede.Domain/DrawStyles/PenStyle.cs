using Eede.Domain.ImageBlenders;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Eede.Domain.DrawStyles
{
    // AlphaOnlyBrushみたいなので実現できるんじゃないの

    public class PenStyle
    {
        public readonly IImageBlender Blender;
        public readonly Color Color;
        public readonly int Width;

        public PenStyle(IImageBlender blender) : this(blender, Color.Black, 1)
        {
        }

        public PenStyle(IImageBlender blender, Color color, int width)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width) + " is 1 or more. then:" + width);
            Blender = blender ?? throw new ArgumentNullException(nameof(blender));
            Color = color;
            Width = width;
        }

        public PenStyle UpdateBlender(IImageBlender blender)
        {
            return new PenStyle(blender, Color, Width);
        }

        public PenStyle UpdateColor(Color color)
        {
            return new PenStyle(Blender, color, Width);
        }

        public PenStyle UpdateWidth(int width)
        {
            return new PenStyle(Blender, Color, width);
        }

        public Pen PreparePen()
        {
            return new Pen(Color)
            {
                Width = Width,
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
        }

        public Brush PrepareBrush()
        {
            return new SolidBrush(Color);
        }
    }
}