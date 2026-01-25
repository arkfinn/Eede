using Eede.Domain.ImageEditing.DrawingTools;
using System;

namespace Eede.Application.Drawings;

public class DrawStyleFactory : IDrawStyleFactory
{
    public IDrawStyle Create(DrawStyleType type)
    {
        return type switch
        {
            DrawStyleType.RegionSelect => new RegionSelector(),
            DrawStyleType.FreeCurve => new FreeCurve(),
            DrawStyleType.Line => new Line(),
            DrawStyleType.Fill => new Fill(),
            DrawStyleType.Rectangle => new Rectangle(),
            DrawStyleType.FilledRectangle => new FilledRectangle(),
            DrawStyleType.Ellipse => new Ellipse(),
            DrawStyleType.FilledEllipse => new FilledEllipse(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown DrawStyle: {type}"),
        };
    }
}
