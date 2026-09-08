using Eede.Domain.SharedKernel;
using System;
using System.Text.Json.Serialization;

namespace Eede.Domain.Animations;

public record GridSettings
{
    public PictureSize CellSize { get; }
    public Position Offset { get; }
    public int Padding { get; }

    [JsonConstructor]
    public GridSettings(PictureSize cellSize, Position offset, int padding)
    {
        if (cellSize.Width <= 0 || cellSize.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(cellSize), "Cell dimensions must be positive.");
        if (offset.X < 0 || offset.Y < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset coordinates must be non-negative.");
        if (padding < 0)
            throw new ArgumentOutOfRangeException(nameof(padding), "Padding must be non-negative.");

        CellSize = cellSize;
        Offset = offset;
        Padding = padding;
    }

    public bool Validate()
    {
        return CellSize.Width > 0 && CellSize.Height > 0 &&
               Offset.X >= 0 && Offset.Y >= 0 &&
               Padding >= 0;
    }

    public int CalculateCellIndex(Position position, PictureSize imageSize)
    {
        if (position.X < Offset.X || position.Y < Offset.Y) return -1;

        int cellStepW = CellSize.Width + Padding;
        int cellStepH = CellSize.Height + Padding;
        if (cellStepW <= 0 || cellStepH <= 0) return -1;

        int columns = Math.Max(1, (imageSize.Width - Offset.X + Padding) / cellStepW);
        int rows = Math.Max(1, (imageSize.Height - Offset.Y + Padding) / cellStepH);

        int col = (position.X - Offset.X) / cellStepW;
        int row = (position.Y - Offset.Y) / cellStepH;

        if (col < 0 || col >= columns || row < 0 || row >= rows) return -1;
        int index = row * columns + col;
        return index < 0 ? -1 : index;
    }
}
