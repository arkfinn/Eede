using Eede.Domain.Palettes;
using System.IO;

namespace Eede.Infrastructure.Palettes.Persistence.ActFileFormat;

public interface IPaletteFileWriter
{
    void Write(Stream stream, Palette palette);
}
