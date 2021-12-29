using Eede.Domain.Pictures;
using System;

namespace Eede.Application.Drawings
{
    public class DrawingResult : IDisposable
    {
        public readonly DrawingBuffer PictureBuffer;
        public readonly DrawingRunner Runner;

        public DrawingResult(DrawingBuffer picture, DrawingRunner runner)
        {
            PictureBuffer = picture;
            Runner = runner;
        }

        public void Dispose()
        {
            PictureBuffer.Dispose();
        }
    }
}