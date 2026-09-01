#nullable enable
using System;
using System.Text.Json.Serialization;

namespace Eede.Domain.SharedKernel;

public readonly record struct PictureArea
{
    [JsonInclude]
    public Position Position { get; }

    [JsonInclude]
    public PictureSize Size { get; }

    [JsonConstructor]
    public PictureArea(Position position, PictureSize size)
    {
        Position = position;
        Size = size;
    }

    [JsonIgnore]
    public int X => Position.X;

    [JsonIgnore]
    public int Y => Position.Y;

    [JsonIgnore]
    public int Width => Size.Width;

    [JsonIgnore]
    public int Height => Size.Height;

    [JsonIgnore]
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

    public PictureArea Combine(PictureArea other)
    {
        if (IsEmpty) return other;
        if (other.IsEmpty) return this;

        int minX = Math.Min(X, other.X);
        int minY = Math.Min(Y, other.Y);
        int maxX = Math.Max(X + Width, other.X + other.Width);
        int maxY = Math.Max(Y + Height, other.Y + other.Height);

        return new PictureArea(new Position(minX, minY), new PictureSize(maxX - minX, maxY - minY));
    }

    public PictureArea Intersect(PictureArea other)
    {
        if (IsEmpty || other.IsEmpty) return new PictureArea(new Position(0, 0), new PictureSize(0, 0));

        int startX = Math.Max(X, other.X);
        int startY = Math.Max(Y, other.Y);
        int endX = Math.Min(X + Width, other.X + other.Width);
        int endY = Math.Min(Y + Height, other.Y + other.Height);

        int width = Math.Max(0, endX - startX);
        int height = Math.Max(0, endY - startY);

        if (width <= 0 || height <= 0)
        {
            return new PictureArea(new Position(0, 0), new PictureSize(0, 0));
        }

        return new PictureArea(new Position(startX, startY), new PictureSize(width, height));
    }
}
