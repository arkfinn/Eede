using Eede.Domain.Drawing;

namespace Eede.Application.Drawings
{
    public class DrawingResult
    {
        public readonly DrawingBuffer PictureBuffer;
        public readonly DrawableArea DrawableArea;

        public DrawingResult(DrawingBuffer picture, DrawableArea runner)
        {
            PictureBuffer = picture;
            DrawableArea = runner;
        }
    }
}
