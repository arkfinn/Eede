using Eede.Domain.ImageEditing;

namespace Eede.Domain.SharedKernel;

/// <summary>
/// UI表示上の座標（ピクセル座標 * 倍率）を表す Value Object。
/// </summary>
public readonly record struct DisplayCoordinate(int X, int Y)
{
    public CanvasCoordinate ToCanvas(Magnification magnification)
    {
        return new CanvasCoordinate(magnification.Minify(X), magnification.Minify(Y));
    }
}