using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing;

public class DrawingSessionTests
{
    private Picture _initialPicture;
    private PictureSize _size = new(32, 32);

    [SetUp]
    public void Setup()
    {
        _initialPicture = Picture.CreateEmpty(_size);
    }

    [Test]
    public void DrawingSession_InitialState()
    {
        var session = new DrawingSession(_initialPicture);
        Assert.Multiple(() =>
        {
            Assert.That(session.CurrentPicture, Is.EqualTo(_initialPicture));
            Assert.That(session.CanUndo(), Is.False);
            Assert.That(session.CanRedo(), Is.False);
        });
    }

    [Test]
    public void DrawingSession_Push_CreatesUndoItem()
    {
        var session = new DrawingSession(_initialPicture);
        var nextPicture = Picture.CreateEmpty(_size); // 本来は異なるデータ
        
        var updatedSession = session.Push(nextPicture);
        
        Assert.Multiple(() =>
        {
            Assert.That(updatedSession.CurrentPicture, Is.EqualTo(nextPicture));
            Assert.That(updatedSession.CanUndo(), Is.True);
        });

        var undoneSession = updatedSession.Undo().Session;
        Assert.That(undoneSession.CurrentPicture, Is.EqualTo(_initialPicture));
    }

    [Test]
    public void DrawingSession_UpdateDrawing_DoesNotAffectUndoStack()
    {
        var session = new DrawingSession(_initialPicture);
        var drawingPicture = Picture.CreateEmpty(_size);
        
        var drawingSession = session.UpdateDrawing(drawingPicture);
        
        Assert.Multiple(() =>
        {
            Assert.That(drawingSession.CurrentPicture, Is.EqualTo(drawingPicture));
            Assert.That(drawingSession.IsDrawing(), Is.True);
            Assert.That(drawingSession.CanUndo(), Is.False, "Cannot undo while drawing");
        });

        var canceledSession = drawingSession.CancelDrawing();
        Assert.That(canceledSession.CurrentPicture, Is.EqualTo(_initialPicture));
        Assert.That(canceledSession.IsDrawing(), Is.False);
    }

    [Test]
    public void DrawingSession_PushAfterDrawing_ResetsDrawingState()
    {
        var session = new DrawingSession(_initialPicture);
        var drawingPicture = Picture.CreateEmpty(_size);
        var finalPicture = Picture.CreateEmpty(_size);

        var s2 = session.UpdateDrawing(drawingPicture).Push(finalPicture);
        
        Assert.Multiple(() =>
        {
            Assert.That(s2.CurrentPicture, Is.EqualTo(finalPicture));
            Assert.That(s2.IsDrawing(), Is.False);
            Assert.That(s2.CanUndo(), Is.True);
        });
    }

    [Test]
    public void DrawingSession_Undo_Restores_SelectingArea()
    {
        // Arrange
        var initialArea = new PictureArea(new Position(0, 0), new PictureSize(10, 10));
        var session = new DrawingSession(_initialPicture, initialArea); // このコンストラクタは未実装
        
        var nextPicture = Picture.CreateEmpty(_size);
        var nextArea = new PictureArea(new Position(5, 5), new PictureSize(15, 15));
        
        // Act
        var updatedSession = session.Push(nextPicture, nextArea); // この引数は未実装
        
        // Assert: Push後の状態
        Assert.That(updatedSession.CurrentSelectingArea, Is.EqualTo(nextArea));
        
        // Act: Undo実行
        var undoneSession = updatedSession.Undo().Session;
        
        // Assert: Undo後の状態が復元されていること
        Assert.Multiple(() =>
        {
            Assert.That(undoneSession.CurrentPicture, Is.EqualTo(_initialPicture));
            Assert.That(undoneSession.CurrentSelectingArea, Is.EqualTo(initialArea));
        });
    }
}
