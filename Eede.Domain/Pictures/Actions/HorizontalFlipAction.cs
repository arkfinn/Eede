using Eede.Domain.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Domain.Pictures.Actions
{
    public class HorizontalFlipAction
    {
        private readonly Picture Source;

        public HorizontalFlipAction(Picture source)
        {
            Source = source;
        }

        public Picture Execute()
        {
            var action = new PictureAction(Source);
            return action.ProcessResult(Source.Size,Offset);
        }

        private Position Offset(Position p)
        {
            return new Position(Source.Width - 1 - p.X, p.Y);
        }
    }
}
