using Eede.Domain.Pictures;

namespace Eede.Domain.Drawing
{
    public static class ContextFactory
    {
        public static DrawingBuffer Create(Picture picture)
        {
            return new DrawingBuffer(picture);
        }
    }
}
