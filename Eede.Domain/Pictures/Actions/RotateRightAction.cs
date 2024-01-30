using Eede.Domain.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Domain.Pictures.Actions
{
    public class RotateRightAction
    {
        private readonly Picture Source;

        public RotateRightAction(Picture source)
        {
            Source = source;
        }

        public Picture Execute()
        {
            var action = new PictureAction(Source);
            return action.ProcessResult(Source.Size.Swap(), Offset);
        }

        private Position Offset(Position p)
        {
            var offsetX = p.Y;// Source.Height-1-p.Y;
            var offsetY = Source.Height - 1 - p.X;
            return new Position(offsetX, offsetY);
        }
    }
}
