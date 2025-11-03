using Eede.Domain.SharedKernel;
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
        int height = destSize.Height;
        int width = destSize.Width;
        int stride = width * 4;
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
                ReadOnlySpan<byte> sourceSpan = Source.AsSpan();
                destPixels[pos + 0] = sourceSpan[fromPos + 0];
                destPixels[pos + 1] = sourceSpan[fromPos + 1];
                destPixels[pos + 2] = sourceSpan[fromPos + 2];
                destPixels[pos + 3] = sourceSpan[fromPos + 3];
            }
        }
        return destPixels;
    }

    public Picture ProcessResult(PictureSize destSize, Func<Position, Position> process)
    {
        byte[] pixels = Process(destSize, process);
        return Picture.Create(destSize, pixels);
    }
}
