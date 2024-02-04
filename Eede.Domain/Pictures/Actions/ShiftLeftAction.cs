using Eede.Domain.Positions;
using Eede.Domain.Scales;
using System;

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
        var action = new PictureAction(Source);
        return action.ProcessResult(Source.Size, Offset);
    }

    private Position Offset(Position p)
    {
        var offsetX = p.X + 1;
        var newX = offsetX >= Source.Width ? offsetX - Source.Width : offsetX;
        return new Position(newX, p.Y);
    }
}