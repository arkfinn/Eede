using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
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

    [Test]
    public void DrawingSession_MovePreview_Undo_Workflow()
    {
        // Arrange: 最初の画像を用意
        var initialPicture = _initialPicture;
        var initialArea = new PictureArea(new Position(0, 0), new PictureSize(10, 10));
        var session = new DrawingSession(initialPicture, initialArea);

        // Act: 移動プレビュー開始 (Cut & Move)
        var movedPixels = Picture.CreateEmpty(new PictureSize(10, 10)); // 実際には切り抜いた画像
        var movedPosition = new Position(5, 5);
        var previewSession = session.UpdatePreviewContent(new SelectionPreviewInfo(movedPixels, movedPosition, SelectionPreviewType.CutAndMove, initialArea));

        // Act: 確定（Commit）
        var committedSession = previewSession.CommitPreview(new DirectImageBlender());

        // Assert: 確定後の状態確認
        Assert.Multiple(() =>
        {
            Assert.That(committedSession.CurrentPreviewContent, Is.Null, "Committed session should have no preview");
            Assert.That(committedSession.CurrentSelectingArea?.Position, Is.EqualTo(movedPosition), "Committed area should be at the new position");
        });

        // Act: Undo (移動確定後の Undo)
        var undoneSession = committedSession.Undo().Session;

        // Assert: Undo後は「移動前」の状態に戻るべき
        Assert.Multiple(() =>
        {
            Assert.That(undoneSession.CurrentPicture, Is.EqualTo(initialPicture), "Should return to the picture before move");
            Assert.That(undoneSession.CurrentPreviewContent, Is.Null, "Undo should NOT restore preview content");
            Assert.That(undoneSession.CurrentSelectingArea, Is.EqualTo(initialArea), "Should return to the selection area before move");
        });
    }

    [Test]
    public void DrawingSession_PastePreview_Undo_Workflow()
    {
        // Arrange: 最初の画像を描画して履歴を1つ作る
        var initialSession = new DrawingSession(_initialPicture);
        var secondPicture = Picture.CreateEmpty(_size); // 異なる内容
        var session = initialSession.Push(secondPicture);

        var pastedPixels = Picture.CreateEmpty(new PictureSize(10, 10));
        var position = new Position(5, 5);

        // Act: ペーストプレビュー開始
        var previewSession = session.PushPastePreview(pastedPixels, position);

        // Assert: プレビュー状態の確認
        Assert.Multiple(() =>
        {
            Assert.That(previewSession.CurrentPreviewContent, Is.Not.Null);
            Assert.That(previewSession.CurrentPreviewContent.Pixels, Is.EqualTo(pastedPixels));
            Assert.That(previewSession.CurrentPreviewContent.Type, Is.EqualTo(SelectionPreviewType.Paste));
            Assert.That(previewSession.CanUndo(), Is.True, "Should be able to undo paste preview when history exists");
        });

        // Act: 確定（Commit）
        var committedSession = previewSession.CommitPreview(new DirectImageBlender());

        // Assert: 確定後の状態確認
        Assert.Multiple(() =>
        {
            Assert.That(committedSession.CurrentPreviewContent, Is.Null);
            Assert.That(committedSession.CanUndo(), Is.True);
        });

        // Act: Undo (ペースト確定後の Undo)
        var undoneSession = committedSession.Undo().Session;
        Assert.That(undoneSession.CurrentPicture, Is.EqualTo(secondPicture));

        // Act: Undo (ペーストプレビュー中の Undo シナリオを模倣)
        // プレビュー状態から直接 Undo した場合、PreviewContent が消えて secondPicture に戻るはず
        var undoneFromPreviewSession = previewSession.Undo().Session;
        Assert.That(undoneFromPreviewSession.CurrentPicture, Is.EqualTo(_initialPicture));
        Assert.That(undoneFromPreviewSession.CurrentPreviewContent, Is.Null, "Undo during preview should clear preview content");
    }

    [Test]
    public void DrawingSession_PastePreview_Cancel()
    {
        // Arrange
        var session = new DrawingSession(_initialPicture);
        var pastedPixels = Picture.CreateEmpty(new PictureSize(10, 10));

        // Act: プレビュー開始 -> キャンセル
        var canceledSession = session.PushPastePreview(pastedPixels, new Position(0, 0)).CancelDrawing();

        // Assert: 元に戻っていること
        Assert.Multiple(() =>
        {
            Assert.That(canceledSession.CurrentPreviewContent, Is.Null);
            Assert.That(canceledSession.CurrentPicture, Is.EqualTo(_initialPicture));
        });
    }
}
