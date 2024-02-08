using Eede.Domain.Positions;
using Eede.Domain.Scales;
using System;

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
        var action = new PictureAction(Source);
        return action.ProcessResult(Source.Size, Offset);
    }

    private Position Offset(Position p)
    {
        var offsetX = p.X - 1;
        var newX = offsetX < 0 ? Source.Width + offsetX : offsetX;
        return new Position(newX, p.Y);
    }
}