using Eede.Domain.Positions;

namespace Eede.Domain.Pictures.Actions
{
    public class ShiftUpAction
    {
        private readonly Picture Source;

        public ShiftUpAction(Picture source)
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
            int offsetY = p.Y + 1;
            int newY = offsetY >= Source.Height ? offsetY - Source.Height : offsetY;
            return new Position(p.X, newY);
        }
    }
}