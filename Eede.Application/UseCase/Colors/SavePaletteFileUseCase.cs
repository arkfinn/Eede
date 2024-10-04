using Eede.Domain.Colors;
using System.IO;

namespace Eede.Application.UseCase.Colors;

public class SavePaletteFileUseCase(AlphaActFileWriter writer)
{
    private readonly AlphaActFileWriter Writer = writer;

    public void Execute(Stream fileStream, Palette palette)
    {
        Writer.Write(fileStream, palette);
    }
}
