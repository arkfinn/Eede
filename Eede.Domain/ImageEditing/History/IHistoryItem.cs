using System.Collections.Generic;
using System.Linq;
using Eede.Domain.SharedKernel;

#nullable enable
namespace Eede.Domain.ImageEditing.History;

public interface IHistoryItem
{
}

public record CanvasHistoryItem(Picture Picture, PictureArea? SelectingArea) : IHistoryItem;

public record PictureDiff(PictureArea Area, Picture Before, Picture? After = null)
{
    public PictureDiff Reverse(Picture currentPicture)
    {
        var after = After ?? currentPicture.CutOut(Area);
        return new PictureDiff(Area, after, Before);
    }
}

public record DiffHistoryItem(IReadOnlyList<PictureDiff> Diffs, PictureArea? SelectingArea) : IHistoryItem
{
    public DiffHistoryItem Reverse(Picture currentPicture, PictureArea? selectingArea)
    {
        var reversed = Diffs.Select(d => d.Reverse(currentPicture)).ToList().AsReadOnly();
        return new DiffHistoryItem(reversed, selectingArea);
    }
}

public record DockActiveHistoryItem(string DockId, Position Position, Picture Before, Picture After, bool BeforeEdited, bool AfterEdited) : IHistoryItem;
