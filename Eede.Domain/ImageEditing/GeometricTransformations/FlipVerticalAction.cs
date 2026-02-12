#nullable enable
using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.GeometricTransformations;

public class FlipVerticalAction
{
    private readonly Picture Source;

    public FlipVerticalAction(Picture source)
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
        return new Position(p.X, Source.Height - 1 - p.Y);
    }
}
