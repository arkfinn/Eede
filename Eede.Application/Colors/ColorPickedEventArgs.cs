using Eede.Domain.Colors;
using System;

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
