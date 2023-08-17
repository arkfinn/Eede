using Eede.Domain.Pictures;
using System;

namespace Eede.Application.Drawings
{
    public class DrawingBuffer : IDisposable
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

        public void Dispose()
        {
        }

        public Picture Fetch()
        {
            if (IsDrawing())
            {
                return Drawing;
            }
            return Previous;
        }

        public bool IsDrawing()
        {
            return (Drawing != null);
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