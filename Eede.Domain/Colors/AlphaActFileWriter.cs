using System.IO;

namespace Eede.Domain.Colors;

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
