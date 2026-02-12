using System.Collections.Generic;
using Eede.Domain.SharedKernel;

#nullable enable
namespace Eede.Domain.ImageEditing.History;

public interface IHistoryItem
{
}

public record CanvasHistoryItem(Picture Picture, PictureArea? SelectingArea) : IHistoryItem;

public record PictureDiff(PictureArea Area, Picture Before, Picture After);

public record DiffHistoryItem(IEnumerable<PictureDiff> Diffs, PictureArea? SelectingArea) : IHistoryItem;

public record DockActiveHistoryItem(string DockId, Position Position, Picture Before, Picture After) : IHistoryItem;