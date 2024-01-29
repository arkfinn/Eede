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
            var action = new PictureAction();
            return Picture.Create(Source.Size, action.Process(Source, OffsetX, OffsetY));
        }

        private int OffsetX(int x)
        {
            return Source.Width -1 - x;
        }

        private int OffsetY(int y)
        {
            return y;
            //return Source.Height-1-y;
        }


    }
}
