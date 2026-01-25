using Eede.Domain.ImageEditing;

namespace Eede.Domain.SharedKernel
{
    public readonly record struct DisplayCoordinate
    {
        public int X { get; }
        public int Y { get; }

        public DisplayCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public CanvasCoordinate ToCanvas(Magnification magnification)
        {
            return new CanvasCoordinate(magnification.Minify(X), magnification.Minify(Y));
        }

        public CanvasCoordinate ToCanvas(Magnification magnification, PictureSize gridSize)
        {
            var canvasX = magnification.Minify(X);
            var canvasY = magnification.Minify(Y);
            var snappedX = (canvasX / gridSize.Width) * gridSize.Width;
            var snappedY = (canvasY / gridSize.Height) * gridSize.Height;
            return new CanvasCoordinate(snappedX, snappedY);
        }
    }
}
