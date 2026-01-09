using Eede.Domain.SharedKernel;

namespace Eede.Domain.Selections;

public record Selection
{
    public PictureArea Area { get; }

    public Selection(PictureArea area)
    {
        Area = area;
    }

    public bool Contains(Position position)
    {
        return Area.X <= position.X && position.X < Area.X + Area.Width &&
               Area.Y <= position.Y && position.Y < Area.Y + Area.Height;
    }

    public Selection Move(int deltaX, int deltaY)
    {
        return new Selection(new PictureArea(new Position(Area.X + deltaX, Area.Y + deltaY), Area.Size));
    }
}
