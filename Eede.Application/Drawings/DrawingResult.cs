using Eede.Domain.Pictures;

namespace Eede.Application.Drawings
{
    public class DrawingResult
    {
        public readonly Picture Picture;
        public readonly DrawingRunner Runner;

        public DrawingResult(Picture picture, DrawingRunner runner)
        {
            Picture = picture;
            Runner = runner;
        }
    }
}