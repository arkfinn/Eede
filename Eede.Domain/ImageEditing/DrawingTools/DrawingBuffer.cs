namespace Eede.Domain.ImageEditing.DrawingTools
{
    internal interface IContext
    {
        bool IsDrawing();
        Picture Fetch();
    }

    internal class Active(Picture now) : IContext
    {
        private readonly Picture Now = now;

        public bool IsDrawing()
        {
            return true;
        }

        public Picture Fetch()
        {
            return Now;
        }
    }

    internal class Idle(Picture previous) : IContext
    {
        private readonly Picture Previous = previous;

        public bool IsDrawing()
        {
            return false;
        }

        public Picture Fetch()
        {
            return Previous;
        }
    }


    public class DrawingBuffer
    {
        public readonly Picture Previous;
        public readonly Picture Drawing;
        private readonly IContext Context;

        public DrawingBuffer(Picture previous)
        {
            Previous = previous;
            Drawing = null;
            Context = new Idle(previous);

        }

        private DrawingBuffer(Picture previous, Picture drawing)
        {
            Previous = previous;
            Drawing = drawing;
            Context = new Active(drawing);
        }

        private DrawingBuffer(Picture previous, Picture drawing, IContext context)
        {
            Previous = previous;
            Drawing = drawing;
            Context = context;
        }


        public Picture Fetch()
        {
            return Context.Fetch();
        }

        public bool IsDrawing()
        {
            return Context.IsDrawing();
        }

        public DrawingBuffer UpdateDrawing(Picture drawing)
        {
            return new DrawingBuffer(Previous, drawing);
        }


        public DrawingBuffer CancelDrawing()
        {
            return new DrawingBuffer(Previous);
        }

        public DrawingBuffer Clone()
        {
            return new DrawingBuffer(Previous, Drawing, Context);
        }

    }
}
