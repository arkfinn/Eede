#nullable enable
using System;

namespace Eede.Domain.Palettes
{
    public sealed class HsvColor
    {
        public static HsvColor FromHsv(int h, int s, int v)
        {
            return new HsvColor(h, s, v);
        }

        public static HsvColor FromRgb(int r, int g, int b)
        {
            int max = Math.Max(Math.Max(r, g), b);
            int min = Math.Min(Math.Min(r, g), b);

            //Vを求める  
            int v = max;
            //RGBすべてが同じ場合　HとSは0     
            if (max == min)
            {
                return new HsvColor(0, 0, v);
            }

            int delta = max - min;

            //Sを求める  
            int s = delta * 255 / max;

            //Hを求める  
            int h = 0;
            if (max == r)
            {
                h = 60 * (g - b) / delta;
                if (h < 0)
                {
                    h += 360;
                }
            }
            else if (max == g)
            {
                h = 60 * (b - r) / delta + 120;
            }
            else if (max == b)
            {
                h = 60 * (r - g) / delta + 240;
            }
            return new HsvColor(h, s, v);
        }

        public readonly int Hue;
        public readonly int Saturation;
        public readonly int Value;
        public HsvColor(int h, int s, int v)
        {
            Hue = Math.Max(0, Math.Min(h, 360));
            Saturation = Math.Max(0, Math.Min(s, 255));
            Value = Math.Max(0, Math.Min(v, 255));
        }

        public ArgbColor ToArgbColor()
        {
            int h = Hue;
            if (h == 360)
            {
                h = 0;
            }

            int Hi = (int)Math.Floor((double)h / 60) % 6;

            float f = (float)h / 60 - Hi;
            float p = (float)Value / 255 * (1 - (float)Saturation / 255);
            float q = (float)Value / 255 * (1 - f * ((float)Saturation / 255));
            float t = (float)Value / 255 * (1 - (1 - f) * ((float)Saturation / 255));

            p *= 255;
            q *= 255;
            t *= 255;

            return Hi switch
            {
                0 => new ArgbColor(255, (byte)Value, (byte)t, (byte)p),
                1 => new ArgbColor(255, (byte)q, (byte)Value, (byte)p),
                2 => new ArgbColor(255, (byte)p, (byte)Value, (byte)t),
                3 => new ArgbColor(255, (byte)p, (byte)q, (byte)Value),
                4 => new ArgbColor(255, (byte)t, (byte)p, (byte)Value),
                5 => new ArgbColor(255, (byte)Value, (byte)p, (byte)q),
                _ => new ArgbColor(0, 0, 0, 0),
            };
        }

        public override bool Equals(object? obj)
        {
            return obj is HsvColor color &&
                   Hue == color.Hue &&
                   Saturation == color.Saturation &&
                   Value == color.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Hue, Saturation, Value);
        }
    }
}
