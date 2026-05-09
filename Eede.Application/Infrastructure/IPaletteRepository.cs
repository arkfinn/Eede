using Eede.Domain.Palettes;

namespace Eede.Application.Infrastructure;

public interface IPaletteRepository
{
    Palette Find(string filePath);
    void Save(Palette palette, string filePath);
}
