using Eede.Domain.SharedKernel;

namespace Eede.Domain.Pictures.Actions
{
    public class FlipHorizontalAction
    {
        private readonly Picture Source;

        public FlipHorizontalAction(Picture source)
        {
            Source = source;
        }

        public Picture Execute()
        {
            PictureAction action = new(Source);
            return action.ProcessResult(Source.Size, Offset);
        }

        private Position Offset(Position p)
        {
            return new Position(Source.Width - 1 - p.X, p.Y);
        }
    }
}
