using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.GeometricTransformations;

public class RotateRightAction
{
    private readonly Picture Source;

    public RotateRightAction(Picture source)
    {
        Source = source;
    }

    public Picture Execute()
    {
        PictureAction action = new(Source);
        return action.ProcessResult(Source.Size.Swap(), Offset);
    }

    private Position Offset(Position p)
    {
        int offsetX = p.Y;// Source.Height-1-p.Y;
        int offsetY = Source.Height - 1 - p.X;
        return new Position(offsetX, offsetY);
    }
}
