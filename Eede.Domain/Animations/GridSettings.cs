using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.Animations;

public record GridSettings(PictureSize CellSize, Position Offset, int Padding)
{
    public int CalculateCellIndex(Position position, PictureSize imageSize)
    {
        int columns = Math.Max(1, (imageSize.Width - Offset.X + Padding) / (CellSize.Width + Padding));
        int col = (position.X - Offset.X) / (CellSize.Width + Padding);
        int row = (position.Y - Offset.Y) / (CellSize.Height + Padding);
        
        if (col < 0 || col >= columns) return -1;
        int index = row * columns + col;
        return index < 0 ? -1 : index;
    }
}
