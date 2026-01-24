using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Domain.Animations;

namespace Eede.Application.Drawings
{
    public interface ICanvasService
    {
        Position Minify(Position displayPosition, Magnification magnification);
        HalfBoxArea GetCurrentHalfBoxArea(Position displayPosition, Magnification magnification, bool isAnimationMode, GridSettings gridSettings, PictureSize defaultGridSize);
    }
}
