using Eede.Domain.SharedKernel;
using System;
using System.Collections.Generic;

namespace Eede.Domain.ImageEditing;

public class BackgroundCalculator
{
    public IEnumerable<LineCoordinate> CalculateVerticalGridLines(PictureSize pictureSize, PictureSize gridSize, Magnification magnification)
    {
        for (int x = gridSize.Width; x < pictureSize.Width; x += gridSize.Width)
        {
            int displayX = magnification.Magnify(x);
            yield return new LineCoordinate(
                new Position(displayX, 0),
                new Position(displayX, magnification.Magnify(pictureSize.Height)));
        }
    }

    public IEnumerable<LineCoordinate> CalculateHorizontalGridLines(PictureSize pictureSize, PictureSize gridSize, Magnification magnification)
    {
        for (int y = gridSize.Height; y < pictureSize.Height; y += gridSize.Height)
        {
            int displayY = magnification.Magnify(y);
            yield return new LineCoordinate(
                new Position(0, displayY),
                new Position(magnification.Magnify(pictureSize.Width), displayY));
        }
    }
}
