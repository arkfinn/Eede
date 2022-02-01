using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Eede.ValueObjects
{
    public sealed class HsvColor
    {
        static public HsvColor FromHsv(int h, int s, int v)
        {
            return new HsvColor(h, s, v);
        }

        static public HsvColor FromRgb(int r, int g, int b)
        {
            double max = Math.Max(Math.Max(r, g), b);
            double min = Math.Min(Math.Min(r, g), b);

            //Vを求める  
            int v = (int)max;
            //RGBすべてが同じ場合　HとSは0     
            if (max == min) return new HsvColor(0, 0, v);

            //Sを求める  
            int s = (int)(((max - min) * 255) / max);

            //Hを求める  
            int h = 0;
            if (max == r)
            {
                h = (int)(60 * (g - b) / (max - min));
                if (h < 0) h += 360;
            }
            else if (max == g)
            {
                h = (int)(60 * (b - r) / (max - min)) + 120;
                if (h < 0) h += 360;
            }
            else if (max == b)
            {
                h = (int)(60 * (r - g) / (max - min)) + 240;
                if (h < 0) h += 360;
            }
            return new HsvColor(h, s, v);
        }

        public readonly int H;
        public readonly int S;
        public readonly int V;
        public HsvColor(int h, int s, int v)
        {
            H = Math.Max(0, Math.Min(h, 360));
            S = Math.Max(0, Math.Min(s, 255));
            V = Math.Max(0, Math.Min(v, 255));
        }

        public Color ToColor()
        {
            int h = H;
            if (h == 360) h = 0;
            int Hi = (int)Math.Floor((double)h / 60) % 6;

            float f = ((float)h / 60) - Hi;
            float p = ((float)V / 255) * (1 - ((float)S / 255));
            float q = ((float)V / 255) * (1 - f * ((float)S / 255));
            float t = ((float)V / 255) * (1 - (1 - f) * ((float)S / 255));

            p *= 255;
            q *= 255;
            t *= 255;

            switch (Hi)
            {
                case 0:
                    return Color.FromArgb(V, (int)t, (int)p);
                case 1:
                    return Color.FromArgb((int)q, V, (int)p);
                case 2:
                    return Color.FromArgb((int)p, V, (int)t);
                case 3:
                    return Color.FromArgb((int)p, (int)q, V);
                case 4:
                    return Color.FromArgb((int)t, (int)p, V);
                case 5:
                    return Color.FromArgb(V, (int)p, (int)q);
            }
            return Color.Empty;
        }

        public override bool Equals(object obj)
        {
            var color = obj as HsvColor;
            return color != null &&
                   H == color.H &&
                   S == color.S &&
                   V == color.V;
        }

        public override int GetHashCode()
        {
            var hashCode = -1397884734;
            hashCode = hashCode * -1521134295 + H.GetHashCode();
            hashCode = hashCode * -1521134295 + S.GetHashCode();
            hashCode = hashCode * -1521134295 + V.GetHashCode();
            return hashCode;
        }
    }
}
