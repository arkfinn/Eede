using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede
{
    public class BitmapEventArgs:EventArgs
    {
        public BitmapEventArgs(Bitmap bitmap)
        {
            Bitmap = bitmap;
        }

        public Bitmap Bitmap { get; private set; }
    }
}
