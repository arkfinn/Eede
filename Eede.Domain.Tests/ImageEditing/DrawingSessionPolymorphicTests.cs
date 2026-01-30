using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.History;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing;

[TestFixture]
public class DrawingSessionPolymorphicTests
{
    private Picture _initialPicture;
    private PictureSize _size = new(32, 32);

    [SetUp]
    public void Setup()
    {
        _initialPicture = Picture.CreateEmpty(_size);
    }

    [Test]
    public void PushDockUpdate_UpdatesHistory_But_Undo_DoesNotRevertCurrentPicture()
    {
        var session = new DrawingSession(_initialPicture);
        var dockId = "dock-1";
        var pos = new Position(10, 10);
        var beforePicture = Picture.CreateEmpty(new PictureSize(10, 10));
        var afterPicture = Picture.CreateEmpty(new PictureSize(10, 10));

        // Act: Push a Dock Update
        var s2 = session.PushDockUpdate(dockId, pos, beforePicture, afterPicture);
        
        // Assert: State immediately after Push
        // Canvas should remain same
        Assert.That(s2.CurrentPicture, Is.EqualTo(_initialPicture));
        Assert.That(s2.CanUndo(), Is.True);
        
        // Act: Undo
        var result = s2.Undo();
        var s3 = result.Session;
        
        // Assert: State after Undo
        // Canvas should STILL be same (Undo of DockUpdate shouldn't affect Canvas)
        Assert.That(s3.CurrentPicture, Is.EqualTo(_initialPicture));
        Assert.That(s3.CanUndo(), Is.False);
    }

    [Test]
    public void Undo_Returns_UndoResult_Containing_UndoneItem()
    {
        var session = new DrawingSession(_initialPicture);
        var dockId = "dock-1";
        var pos = new Position(10, 10);
        var beforePicture = Picture.CreateEmpty(new PictureSize(10, 10));
        var afterPicture = Picture.CreateEmpty(new PictureSize(10, 10));

        var s2 = session.PushDockUpdate(dockId, pos, beforePicture, afterPicture);
        
        // Act: Undo
        // We expect Undo() to return a record containing the new session and the undone item.
        UndoResult result = s2.Undo();
        
        Assert.That(result.Session.CanUndo(), Is.False);
        Assert.That(result.Item, Is.InstanceOf<DockActiveHistoryItem>());
        var undoneItem = result.Item as DockActiveHistoryItem;
        Assert.That(undoneItem.DockId, Is.EqualTo(dockId));
        Assert.That(undoneItem.Before, Is.EqualTo(beforePicture));
        Assert.That(undoneItem.After, Is.EqualTo(afterPicture));
    }

    [Test]
    public void Redo_Restores_DockUpdate()
    {
        var session = new DrawingSession(_initialPicture);
        var dockId = "dock-1";
        var pos = new Position(10, 10);
        var beforePicture = Picture.CreateEmpty(new PictureSize(10, 10));
        var afterPicture = Picture.CreateEmpty(new PictureSize(10, 10));

        var s2 = session.PushDockUpdate(dockId, pos, beforePicture, afterPicture);
        var s3 = s2.Undo().Session;
        
        // Act: Redo
        var result = s3.Redo();
        
        Assert.That(result.Session.CanUndo(), Is.True);
        Assert.That(result.Item, Is.InstanceOf<DockActiveHistoryItem>());
        var redoneItem = result.Item as DockActiveHistoryItem;
        Assert.That(redoneItem.DockId, Is.EqualTo(dockId));
        Assert.That(redoneItem.Before, Is.EqualTo(beforePicture));
        Assert.That(redoneItem.After, Is.EqualTo(afterPicture));
    }
}
