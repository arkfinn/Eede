using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.GeometricTransformations;

public class ShiftDownAction
{
    private readonly Picture Source;

    public ShiftDownAction(Picture source)
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
        int offsetY = p.Y - 1;
        int newY = offsetY < 0 ? Source.Height + offsetY : offsetY;
        return new Position(p.X, newY);
    }
}