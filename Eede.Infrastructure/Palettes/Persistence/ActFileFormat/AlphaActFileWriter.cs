using Eede.Domain.Palettes;
using System.IO;

namespace Eede.Infrastructure.Palettes.Persistence.ActFileFormat;

public class AlphaActFileWriter
{

    public void Write(Stream stream, Palette palette)
    {
        palette.ForEach((color, no) =>
        {
            stream.WriteByte(color.Red);
            stream.WriteByte(color.Green);
            stream.WriteByte(color.Blue);
            stream.WriteByte(color.Alpha);
        });
    }
}
