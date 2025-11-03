using Eede.Domain.SharedKernel;

namespace Eede.Domain.Pictures.Actions;

public class ShiftRightAction
{
    private readonly Picture Source;

    public ShiftRightAction(Picture source)
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
        int offsetX = p.X - 1;
        int newX = offsetX < 0 ? Source.Width + offsetX : offsetX;
        return new Position(newX, p.Y);
    }
}