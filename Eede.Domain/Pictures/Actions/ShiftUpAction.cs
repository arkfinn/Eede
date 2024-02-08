using Eede.Domain.Positions;
using Eede.Domain.Scales;
using System;

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
            var action = new PictureAction(Source);
            return action.ProcessResult(Source.Size, Offset);
        }

        private Position Offset(Position p)
        {
            var offsetY = p.Y + 1;
            var newY = offsetY >= Source.Height ? offsetY - Source.Height : offsetY;
            return new Position(p.X, newY);
        }
    }
}