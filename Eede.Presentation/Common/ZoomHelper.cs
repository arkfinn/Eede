#nullable enable
using System;
using Avalonia;
using Eede.Domain.ImageEditing;

namespace Eede.Presentation.Common
{
    public static class ZoomHelper
    {
        public static readonly float[] MagnificationSteps = [1f, 2f, 4f, 6f, 8f, 12f];

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

        public static Vector CalculateZoomOffset(Vector oldOffset, Point pointerInViewport, float oldMag, float newMag)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(oldMag);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newMag);

            double ratio = (double)newMag / oldMag;
            double newX = (oldOffset.X + pointerInViewport.X) * ratio - pointerInViewport.X;
            double newY = (oldOffset.Y + pointerInViewport.Y) * ratio - pointerInViewport.Y;
            return new Vector(Math.Max(0, newX), Math.Max(0, newY));
        }

        public static Vector CalculatePanOffset(Vector startOffset, Vector delta)
        {
            double newX = startOffset.X - delta.X;
            double newY = startOffset.Y - delta.Y;
            return new Vector(Math.Max(0, newX), Math.Max(0, newY));
        }
    }
}
