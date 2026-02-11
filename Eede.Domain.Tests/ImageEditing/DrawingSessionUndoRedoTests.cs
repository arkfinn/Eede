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
}
