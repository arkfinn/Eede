﻿using Eede.Domain.Colors;
using System;

namespace Eede.Application.Colors
{
    public class ColorPickedEventArgs(ArgbColor newColor) : EventArgs
    {
        public readonly ArgbColor NewColor = newColor;
    }
}
