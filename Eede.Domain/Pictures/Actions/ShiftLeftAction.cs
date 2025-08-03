using Eede.Domain.Positions;
using Eede.Domain.Pictures;

namespace Eede.Domain.Pictures.Actions;

public class ShiftLeftAction
{
    private readonly Picture Source;

    public ShiftLeftAction(Picture source)
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
        int offsetX = p.X + 1;
        int newX = offsetX >= Source.Width ? offsetX - Source.Width : offsetX;
        return new Position(newX, p.Y);
    }
}