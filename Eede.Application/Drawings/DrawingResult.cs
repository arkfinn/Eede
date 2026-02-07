using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;

namespace Eede.Application.Drawings
{
    public class DrawingResult
    {
        public readonly DrawingBuffer PictureBuffer;
        public readonly DrawableArea DrawableArea;
        public readonly PictureRegion AffectedArea;

        public DrawingResult(DrawingBuffer picture, DrawableArea runner, PictureRegion affectedArea = default)
        {
            PictureBuffer = picture;
            DrawableArea = runner;
            AffectedArea = affectedArea;
        }
    }
}
