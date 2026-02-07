using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.History;

public interface IHistoryItem
{
}

public record CanvasHistoryItem(Picture Picture, PictureArea? SelectingArea) : IHistoryItem;

public record DiffHistoryItem(PictureArea Area, Picture Before, Picture After, PictureArea? SelectingArea) : IHistoryItem;

public record DockActiveHistoryItem(string DockId, Position Position, Picture Before, Picture After) : IHistoryItem;
