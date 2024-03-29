﻿using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System;

namespace Eede.Domain.ImageBlenders
{
    public class DirectImageBlender : IImageBlender
    {
        public Picture Blend(Picture from, Picture to)
        {
            return Blend(from, to, new Position(0, 0));
        }

        public Picture Blend(Picture from, Picture to, Position toPosition)
        {
            byte[] toPixels = to.CloneImage();

            int maxY = Math.Min(toPosition.Y + from.Height, to.Height);
            int maxX = Math.Min(toPosition.X + from.Width, to.Width);

            for (int y = toPosition.Y; y < maxY; y++)
            {
                for (int x = toPosition.X; x < maxX; x++)
                {
                    int toPos = (x * 4) + (to.Stride * y);
                    int fromPos = ((x - toPosition.X) * 4) + (from.Stride * (y - toPosition.Y));
                    toPixels[toPos + 0] = from[fromPos + 0];
                    toPixels[toPos + 1] = from[fromPos + 1];
                    toPixels[toPos + 2] = from[fromPos + 2];
                    toPixels[toPos + 3] = from[fromPos + 3];
                }
            }
            return Picture.Create(to.Size, toPixels);
        }
    }
}