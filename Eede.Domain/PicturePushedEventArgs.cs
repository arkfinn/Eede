using Eede.Positions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede
{
    public class PicturePushedEventArgs : EventArgs
    {
        public PicturePushedEventArgs(Bitmap graphics, Position position)
        {
            Bitmap = graphics ?? throw new ArgumentNullException("graphics needs not null.");
            Position = position ?? throw new ArgumentNullException("position needs not null.");
        }

        public readonly Bitmap Bitmap;
        public readonly Position Position;
    }
}
