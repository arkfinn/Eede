using Eede.Domain.ImageEditing;

namespace Eede.Domain.SharedKernel;

/// <summary>
/// キャンバス上のピクセル座標を表す Value Object。
/// </summary>
public readonly record struct CanvasCoordinate(int X, int Y)
{
    public Position ToPosition() => new(X, Y);

    public DisplayCoordinate ToDisplay(Magnification magnification)
    {
        return new DisplayCoordinate(magnification.Magnify(X), magnification.Magnify(Y));
    }
}