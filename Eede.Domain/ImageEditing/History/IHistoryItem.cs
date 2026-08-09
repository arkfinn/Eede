using System.Collections.Generic;
using System.Linq;
using Eede.Domain.SharedKernel;

#nullable enable
namespace Eede.Domain.ImageEditing.History;

public interface IHistoryItem
{
}

public record CanvasHistoryItem(Picture Picture, PictureArea? SelectingArea) : IHistoryItem;

public record PictureDiff(PictureArea Area, Picture Before, Picture After)
{
    public PictureDiff Reverse() => new PictureDiff(Area, After, Before);
}

public record DiffHistoryItem(IReadOnlyList<PictureDiff> Diffs, PictureArea? SelectingArea) : IHistoryItem
{
    private IReadOnlyList<PictureDiff>? _reversedDiffs;

    private IReadOnlyList<PictureDiff> ReversedDiffs =>
        _reversedDiffs ??= Diffs.Select(d => d.Reverse()).ToList().AsReadOnly();

    public DiffHistoryItem Reverse(PictureArea? selectingArea)
    {
        return new DiffHistoryItem(ReversedDiffs, selectingArea) { _reversedDiffs = Diffs };
    }
}

public record DockActiveHistoryItem(string DockId, Position Position, Picture Before, Picture After, bool BeforeEdited, bool AfterEdited) : IHistoryItem;
