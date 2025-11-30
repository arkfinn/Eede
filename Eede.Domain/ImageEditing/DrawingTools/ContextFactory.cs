namespace Eede.Domain.ImageEditing.DrawingTools
{
    public static class ContextFactory
    {
        public static DrawingBuffer Create(Picture picture)
        {
            return new DrawingBuffer(picture);
        }
    }
}
