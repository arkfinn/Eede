﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Domain.Colors;

public class PaletteFileReader : IPaletteFileReader
{
    public Palette Read(Stream fileStream)
    {
        int colorCount = Palette.MAX_LENGTH;
        ArgbColor[] palette = new ArgbColor[colorCount];

        using (BinaryReader reader = new(fileStream))
        {
            for (int i = 0; i < colorCount; i++)
            {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                byte a = reader.ReadByte();

                palette[i] = new ArgbColor(a, r, g, b);
            }
        }

        return Palette.FromColors(palette);
    }
}
