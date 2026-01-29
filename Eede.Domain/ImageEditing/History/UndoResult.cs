namespace Eede.Domain.ImageEditing.History;

public record UndoResult(DrawingSession Session, IHistoryItem? Item);

public record RedoResult(DrawingSession Session, IHistoryItem? Item);
