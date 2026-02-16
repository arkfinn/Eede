#nullable enable
using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.GeometricTransformations;

public class MirrorCopyRightAction
{
    private readonly Picture Source;

    public MirrorCopyRightAction(Picture source)
    {
        Source = source;
    }

    public Picture Execute()
    {
        PictureAction action = new(Source);
        return action.ProcessResult(Source.Size, Offset);
    }

    private Position Offset(Position p)
    {
        int limit = (Source.Width - 1) / 2;
        if (p.X <= limit)
        {
            return p;
        }
        return new Position(Source.Width - 1 - p.X, p.Y);
    }
}
