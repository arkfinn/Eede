using Eede.Domain.Drawings;
using System;

namespace Eede.Application.Drawings
{
    public class DrawingResult : IDisposable
    {
        public readonly DrawingBuffer PictureBuffer;
        public readonly DrawableArea DrawableArea;

        public DrawingResult(DrawingBuffer picture, DrawableArea runner)
        {
            PictureBuffer = picture;
            DrawableArea = runner;
        }

        public void Dispose()
        {
            PictureBuffer.Dispose();
        }
    }
}