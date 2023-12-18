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
            var action = new PictureAction();
            return Picture.Create(Source.Size, action.Process(Source, OffsetX, OffsetY));
        }

        private int OffsetY(int y)
        {
            //return y;
            var offsetY = y + 1;
            return offsetY >= Source.Height ? offsetY - Source.Height : offsetY;
            //var offsetY = y - 1;
            //return offsetY < 0 ? offsetY + Source.Height : offsetY;
        }

        private int OffsetX(int x)
        { 
            //var offsetX = x + 1;
            //return offsetX >= Source.Height ? offsetX - Source.Height : offsetX;

            return x;
        }
    }
}