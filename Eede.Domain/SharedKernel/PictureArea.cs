using System;

namespace Eede.Domain.SharedKernel;

public readonly record struct PictureArea
{
    public readonly Position Position;
    public readonly PictureSize Size;

    public PictureArea(Position position, PictureSize size)
    {
        Position = position;
        Size = size;
    }

    public int X => Position.X;
    public int Y => Position.Y;
    public int Width => Size.Width;
    public int Height => Size.Height;

    public bool IsEmpty => Width <= 0 || Height <= 0;

    public static PictureArea FromPosition(Position from, Position to, PictureSize limit)
    {
        int startX = Math.Min(from.X, to.X);
        int endX = Math.Max(from.X, to.X);
        int startY = Math.Min(from.Y, to.Y);
        int endY = Math.Max(from.Y, to.Y);

        int clampedStartX = Math.Max(0, Math.Min(startX, limit.Width));
        int clampedEndX = Math.Max(0, Math.Min(endX, limit.Width));
        int clampedStartY = Math.Max(0, Math.Min(startY, limit.Height));
        int clampedEndY = Math.Max(0, Math.Min(endY, limit.Height));

        int width = clampedEndX - clampedStartX;
        int height = clampedEndY - clampedStartY;

        return new PictureArea(new Position(clampedStartX, clampedStartY), new PictureSize(width, height));
    }
}
