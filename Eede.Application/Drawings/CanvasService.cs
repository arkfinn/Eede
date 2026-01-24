using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Domain.Animations;

namespace Eede.Application.Drawings
{
    public class CanvasService : ICanvasService
    {
        public Position Minify(Position displayPosition, Magnification magnification)
        {
            return new Position(magnification.Minify(displayPosition.X), magnification.Minify(displayPosition.Y));
        }

        public HalfBoxArea GetCurrentHalfBoxArea(Position displayPosition, Magnification magnification, bool isAnimationMode, GridSettings gridSettings, PictureSize defaultGridSize)
        {
            Position minified = Minify(displayPosition, magnification);
            if (isAnimationMode)
            {
                return HalfBoxArea.Create(minified, gridSettings.CellSize);
            }
            return HalfBoxArea.Create(minified, defaultGridSize);
        }
    }
}
