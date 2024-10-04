using System.IO;

namespace Eede.Domain.Colors
{
    public interface IPaletteFileReader
    {
        Palette Read(Stream fileStream);
    }
}