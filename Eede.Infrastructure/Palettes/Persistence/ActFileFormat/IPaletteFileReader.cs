using Eede.Domain.Palettes;
using System.IO;

namespace Eede.Infrastructure.Palettes.Persistence.ActFileFormat
{
    public interface IPaletteFileReader
    {
        Palette Read(Stream fileStream);
    }
}