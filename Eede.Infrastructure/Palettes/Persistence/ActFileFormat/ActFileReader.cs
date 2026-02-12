using Eede.Domain.Palettes;
using System.IO;

namespace Eede.Infrastructure.Palettes.Persistence.ActFileFormat;

public class ActFileReader : IPaletteFileReader
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

                palette[i] = new ArgbColor(255, r, g, b);
            }
        }

        return Palette.FromColors(palette);
    }
}
