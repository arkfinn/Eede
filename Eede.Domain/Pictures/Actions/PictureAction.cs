using Eede.Domain.Positions;
using System;

namespace Eede.Domain.Pictures.Actions;

internal class PictureAction
{
    private readonly Picture Source;

    public PictureAction(Picture source)
    {
        Source = source;
    }

    public byte[] Process(PictureSize destSize, Func<Position, Position> process)
    {
        var height = destSize.Height;
        var width = destSize.Width;
        var stride = width * 4;
        byte[] destPixels = new byte[stride * height];

        for (int y = 0; y < height; y++)
        {
            int toY = stride * y;

            for (int x = 0; x < width; x++)
            {
                int pos = (x * 4) + toY;
                Position offset = process(new Position(x, y));
                int offsetY = Source.Stride * offset.Y;
                int fromPos = (offset.X * 4) + offsetY;
                Console.WriteLine(x.ToString() + "," + y.ToString());
                Console.WriteLine(offset.X + "," + offset.Y);
                destPixels[pos + 0] = Source[fromPos + 0];
                destPixels[pos + 1] = Source[fromPos + 1];
                destPixels[pos + 2] = Source[fromPos + 2];
                destPixels[pos + 3] = Source[fromPos + 3];
            }
        }
        return destPixels;
    }

    public Picture ProcessResult(PictureSize destSize, Func<Position, Position> process)
    {
        var pixels = Process(destSize, process);
        return Picture.Create(destSize, pixels);
    }
}
