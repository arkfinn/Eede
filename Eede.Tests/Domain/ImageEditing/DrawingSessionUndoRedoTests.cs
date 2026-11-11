using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.ImageEditing;

public class DrawingSessionUndoRedoTests
{
    [Test]
    public void UndoRedo_ShouldRestoreFullPicture()
    {
        var initialPixels = new byte[16]; // 2x2
        var initialPicture = Picture.Create(new PictureSize(2, 2), initialPixels);
        var session = new DrawingSession(initialPicture);

        var nextPixels = new byte[16];
        nextPixels[0] = 255; // Change one pixel
        var nextPicture = Picture.Create(new PictureSize(2, 2), nextPixels);

        // Push new state
        var sessionAfterPush = session.Push(nextPicture);
        Assert.That(sessionAfterPush.CurrentPicture.AsSpan()[0], Is.EqualTo(255));
        Assert.That(sessionAfterPush.CanUndo(), Is.True);

        // Undo
        var undoResult = sessionAfterPush.Undo();
        var sessionAfterUndo = undoResult.Session;
        Assert.That(sessionAfterUndo.CurrentPicture.AsSpan()[0], Is.EqualTo(0));
        Assert.That(sessionAfterUndo.CanRedo(), Is.True);

        // Redo
        var redoResult = sessionAfterUndo.Redo();
        var sessionAfterRedo = redoResult.Session;
        Assert.That(sessionAfterRedo.CurrentPicture.AsSpan()[0], Is.EqualTo(255));
    }

    [Test]
    public void UndoRedo_WithPushDiff_ShouldRestorePartialArea()
    {
        // 4x4 の透明な画像 (stride = 16)
        var initialPixels = new byte[64];
        var initialPicture = Picture.Create(new PictureSize(4, 4), initialPixels);
        var session = new DrawingSession(initialPicture);

        // (1, 1) から 2x2 の領域を 128 で塗りつぶす
        var afterPixels = new byte[16];
        for (int i = 0; i < 16; i++) afterPixels[i] = 128;
        var diffArea = new PictureArea(new Position(1, 1), new PictureSize(2, 2));
        var nextPicture = initialPicture.Blend(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), Picture.Create(diffArea.Size, afterPixels), diffArea.Position);

        // PushDiff を実行
        var sessionAfterPush = session.PushDiff(nextPicture, diffArea);

        // (1, 1) のピクセルが 128 になっていることを確認
        // Position(1, 1) -> index = (1 * 4) + (1 * 16) = 20
        Assert.That(sessionAfterPush.CurrentPicture.AsSpan()[20], Is.EqualTo(128));
        Assert.That(sessionAfterPush.CanUndo(), Is.True);

        // Undo
        var undoResult = sessionAfterPush.Undo();
        var sessionAfterUndo = undoResult.Session;
        Assert.That(sessionAfterUndo.CurrentPicture.AsSpan()[20], Is.EqualTo(0));
        Assert.That(sessionAfterUndo.CanRedo(), Is.True);

        // Redo
        var redoResult = sessionAfterUndo.Redo();
        var sessionAfterRedo = redoResult.Session;
        Assert.That(sessionAfterRedo.CurrentPicture.AsSpan()[20], Is.EqualTo(128));
    }

    [Test]
    public void DefaultMaxUndoCount_ShouldBe200()
    {
        var session = new DrawingSession(Picture.CreateEmpty(new PictureSize(2, 2)));
        Assert.That(session.MaxUndoCount, Is.EqualTo(200));
        Assert.That(session.UndoCount, Is.EqualTo(0));
    }

    [Test]
    public void UndoLimit_ShouldDropOldestItem_WhenExceedingLimit()
    {
        var initialPixels = new byte[16]; // 2x2
        var initialPicture = Picture.Create(new PictureSize(2, 2), initialPixels);
        var session = new DrawingSession(initialPicture, maxUndoCount: 3);

        // 4回変更をPushする (値: 10, 20, 30, 40)
        for (byte i = 1; i <= 4; i++)
        {
            var p = new byte[16];
            p[0] = (byte)(i * 10);
            session = session.Push(Picture.Create(new PictureSize(2, 2), p));
        }

        // 現在値は 40、UndoCount は上限の 3
        Assert.That(session.CurrentPicture.AsSpan()[0], Is.EqualTo(40));
        Assert.That(session.UndoCount, Is.EqualTo(3));

        // 1回目 Undo -> 30
        var u1 = session.Undo();
        Assert.That(u1.Session.CurrentPicture.AsSpan()[0], Is.EqualTo(30));
        Assert.That(u1.Session.UndoCount, Is.EqualTo(2));
        Assert.That(u1.Session.CanUndo(), Is.True);

        // 2回目 Undo -> 20
        var u2 = u1.Session.Undo();
        Assert.That(u2.Session.CurrentPicture.AsSpan()[0], Is.EqualTo(20));
        Assert.That(u2.Session.UndoCount, Is.EqualTo(1));
        Assert.That(u2.Session.CanUndo(), Is.True);

        // 3回目 Undo -> 10
        var u3 = u2.Session.Undo();
        Assert.That(u3.Session.CurrentPicture.AsSpan()[0], Is.EqualTo(10));
        Assert.That(u3.Session.UndoCount, Is.EqualTo(0));

        // 上限 3 なので、最古の初期状態 0 への復元は破棄されており Undo 不可
        Assert.That(u3.Session.CanUndo(), Is.False);
    }

    [Test]
    public void PushDiff_ShouldStoreOnlyBefore_AndSupportMultipleUndoRedoCycles()
    {
        // 4x4 初期画像 (全ピクセル 0)
        var initialPixels = new byte[64];
        var initialPicture = Picture.Create(new PictureSize(4, 4), initialPixels);
        var session = new DrawingSession(initialPicture);

        // 2x2 領域 (1, 1) を 200 に変更
        var p200 = new byte[16];
        for (int i = 0; i < 16; i++) p200[i] = 200;
        var diffArea = new PictureArea(new Position(1, 1), new PictureSize(2, 2));
        var nextPicture = initialPicture.Blend(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), Picture.Create(diffArea.Size, p200), diffArea.Position);

        var pushedSession = session.PushDiff(nextPicture, diffArea);

        // 1回目の Undo -> 0 に戻る
        var u1 = pushedSession.Undo();
        Assert.That(u1.Session.CurrentPicture.AsSpan()[20], Is.EqualTo(0));
        // Undo結果の Item から、元の DiffHistoryItem の After が null だった（メモリ節約されていた）ことを検証
        var diffItem = u1.Item as Eede.Domain.ImageEditing.History.DiffHistoryItem;
        Assert.That(diffItem, Is.Not.Null);
        Assert.That(diffItem!.Diffs[0].After, Is.Null, "Initial PushDiff should not allocate After picture to save memory");

        // 1回目の Redo -> 200 に復元
        var r1 = u1.Session.Redo();
        Assert.That(r1.Session.CurrentPicture.AsSpan()[20], Is.EqualTo(200));

        // 2回目の Undo -> 再び 0 に戻る
        var u2 = r1.Session.Undo();
        Assert.That(u2.Session.CurrentPicture.AsSpan()[20], Is.EqualTo(0));

        // 2回目の Redo -> 再び 200 に復元
        var r2 = u2.Session.Redo();
        Assert.That(r2.Session.CurrentPicture.AsSpan()[20], Is.EqualTo(200));
    }
}
