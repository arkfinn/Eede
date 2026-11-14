#nullable enable
using System;
using Avalonia;
using Eede.Domain.ImageEditing;

namespace Eede.Presentation.Common
{
    public static class ZoomHelper
    {
        public static readonly float[] MagnificationSteps = [1f, 2f, 4f, 6f, 8f, 12f, 16f];

        public static Magnification CalculateNextMagnification(Magnification current, int deltaY)
        {
            if (deltaY > 0)
            {
                foreach (float step in MagnificationSteps)
                {
                    if (step > current.Value)
                    {
                        return new Magnification(step);
                    }
                }
                return current;
            }
            else if (deltaY < 0)
            {
                for (int i = MagnificationSteps.Length - 1; i >= 0; i--)
                {
                    if (MagnificationSteps[i] < current.Value)
                    {
                        return new Magnification(MagnificationSteps[i]);
                    }
                }
                return current;
            }
            return current;
        }

        public static Vector CalculateOverscrollMargin(Size viewportSize, double minMargin = 32.0)
        {
            double mx = Math.Max(minMargin, Math.Ceiling(viewportSize.Width / 32.0) * 16.0);
            double my = Math.Max(minMargin, Math.Ceiling(viewportSize.Height / 32.0) * 16.0);
            return new Vector(mx, my);
        }

        public static Vector CalculateZoomOffset(Vector oldOffset, Point pointerInViewport, float oldMag, float newMag, Vector margin = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(oldMag);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newMag);

            double ratio = (double)newMag / oldMag;
            double newX = margin.X + ((oldOffset.X + pointerInViewport.X - margin.X) * ratio) - pointerInViewport.X;
            double newY = margin.Y + ((oldOffset.Y + pointerInViewport.Y - margin.Y) * ratio) - pointerInViewport.Y;
            return new Vector(Math.Max(0, newX), Math.Max(0, newY));
        }

        public static Vector CalculateInitialOffset(Vector margin, Size displaySize, Size viewportSize)
        {
            double initX = displaySize.Width <= viewportSize.Width
                ? margin.X + (displaySize.Width - viewportSize.Width) / 2.0
                : margin.X;

            double initY = displaySize.Height <= viewportSize.Height
                ? margin.Y + (displaySize.Height - viewportSize.Height) / 2.0
                : margin.Y;

            return new Vector(Math.Max(0, initX), Math.Max(0, initY));
        }

        public static Vector CalculatePanOffset(Vector startOffset, Vector delta)
        {
            double newX = startOffset.X - delta.X;
            double newY = startOffset.Y - delta.Y;
            return new Vector(Math.Max(0, newX), Math.Max(0, newY));
        }
    }
}
