#nullable enable
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.History;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.History;

public class UndoResultTests
{
    [Test]
    public void UndoResult_AssignsPropertiesCorrectly()
    {
        var session = new DrawingSession(Picture.CreateEmpty(new PictureSize(10, 10)));
        IHistoryItem? item = new CanvasHistoryItem(Picture.CreateEmpty(new PictureSize(10, 10)), null);

        var undoResult = new UndoResult(session, item);

        Assert.That(undoResult.Session, Is.SameAs(session));
        Assert.That(undoResult.Item, Is.SameAs(item));
    }

    [Test]
    public void UndoResult_WithNullItem_AssignsPropertiesCorrectly()
    {
        var session = new DrawingSession(Picture.CreateEmpty(new PictureSize(10, 10)));

        var undoResult = new UndoResult(session, null);

        Assert.That(undoResult.Session, Is.SameAs(session));
        Assert.That(undoResult.Item, Is.Null);
    }

    [Test]
    public void RedoResult_AssignsPropertiesCorrectly()
    {
        var session = new DrawingSession(Picture.CreateEmpty(new PictureSize(10, 10)));
        IHistoryItem? item = new CanvasHistoryItem(Picture.CreateEmpty(new PictureSize(10, 10)), null);

        var redoResult = new RedoResult(session, item);

        Assert.That(redoResult.Session, Is.SameAs(session));
        Assert.That(redoResult.Item, Is.SameAs(item));
    }

    [Test]
    public void RedoResult_WithNullItem_AssignsPropertiesCorrectly()
    {
        var session = new DrawingSession(Picture.CreateEmpty(new PictureSize(10, 10)));

        var redoResult = new RedoResult(session, null);

        Assert.That(redoResult.Session, Is.SameAs(session));
        Assert.That(redoResult.Item, Is.Null);
    }
}
