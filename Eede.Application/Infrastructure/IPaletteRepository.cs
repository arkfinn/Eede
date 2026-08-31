using Eede.Domain.Palettes;
using System.IO;

namespace Eede.Application.Infrastructure;

public interface IPaletteRepository
{
    Palette Find(string filePath);
    Palette Find(Stream stream, string extension);
    void Save(Palette palette, string filePath);
    void Save(Palette palette, Stream stream, string extension);
}
