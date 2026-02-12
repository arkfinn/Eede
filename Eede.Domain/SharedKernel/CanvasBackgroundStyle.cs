#nullable enable
using System;
using Eede.Domain.Palettes;

namespace Eede.Domain.SharedKernel;

public readonly record struct CanvasBackgroundStyle
{
    public readonly bool ShowGrid;
    public readonly ArgbColor GridColor;
    public readonly ArgbColor Color1;
    public readonly ArgbColor Color2;

    public CanvasBackgroundStyle(bool showGrid, ArgbColor gridColor, ArgbColor color1, ArgbColor color2)
    {
        ShowGrid = showGrid;
        GridColor = gridColor;
        Color1 = color1;
        Color2 = color2;
    }

    public static CanvasBackgroundStyle Default => new(
        true,
        new ArgbColor(255, 200, 200, 200),
        new ArgbColor(255, 255, 255, 255),
        new ArgbColor(255, 240, 240, 240));
}
