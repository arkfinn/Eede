using Eede.Domain.Colors;
using Eede.Domain.Pictures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Application.Colors
{
    public class ColorPickedEventArgs : EventArgs
    {
        public ColorPickedEventArgs(ArgbColor newColor)
        {
            NewColor = newColor ?? throw new ArgumentNullException(nameof(newColor));
        }

        public readonly ArgbColor NewColor;
    }
}
