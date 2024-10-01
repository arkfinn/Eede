using Eede.Domain.Colors;
using System.IO;

namespace Eede.Application.UseCase.Colors;

public class LoadPaletteFileUseCase(IPaletteFileReader reader)
{
    private readonly IPaletteFileReader Reader = reader;

    public Palette Execute(Stream fileStream)
    {
        return Reader.Read(fileStream);
    }
}
