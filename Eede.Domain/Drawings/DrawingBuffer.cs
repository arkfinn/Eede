using Eede.Domain.Pictures;

namespace Eede.Domain.Drawings
{
    public class DrawingBuffer
    {
        public readonly Picture Previous;
        public readonly Picture Drawing;

        public DrawingBuffer(Picture previous)
        {
            Previous = previous;
            Drawing = null;
        }

        public DrawingBuffer(Picture previous, Picture drawing)
        {
            Previous = previous;
            Drawing = drawing;
        }


        public Picture Fetch()
        {
            return IsDrawing() ? Drawing : Previous;
        }

        public bool IsDrawing()
        {
            return Drawing != null;
        }

        public DrawingBuffer UpdateDrawing(Picture drawing)
        {
            return new DrawingBuffer(Previous, drawing);
        }

        public DrawingBuffer DecideDrawing(Picture drawing)
        {
            return new DrawingBuffer(drawing);
        }

        public DrawingBuffer CancelDrawing()
        {
            return new DrawingBuffer(Previous);
        }

        public DrawingBuffer Clone()
        {
            return new DrawingBuffer(Previous, Drawing);
        }
    }
}