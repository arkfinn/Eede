using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.SelectionStates;

[TestFixture]
public class DraggingStateTests
{
    private Picture _samplePicture;
    private PictureArea _originalArea;
    private Position _startPosition;
    private DraggingState _state;

    [SetUp]
    public void SetUp()
    {
        _samplePicture = Picture.Create(new PictureSize(10, 10), new byte[10 * 10 * 4]);
        _originalArea = new PictureArea(new Position(5, 5), new PictureSize(10, 10));
        _startPosition = new Position(10, 10);
        _state = new DraggingState(_samplePicture, _originalArea, _startPosition);
    }

    [Test]
    public void HandlePointerMoved_UpdatesPreviewInfoPosition()
    {
        var cursorArea = HalfBoxArea.Create(_startPosition, new PictureSize(8, 8));
        var canvasSize = new PictureSize(100, 100);

        // Move by (+15, +20) => now at (25, 30)
        _state.HandlePointerMoved(cursorArea, true, new Position(25, 30), false, canvasSize);

        var info = _state.GetSelectionPreviewInfo();
        Assert.That(info, Is.Not.Null);
        // Original (5, 5) + offset (15, 20) = (20, 25)
        Assert.That(info!.Position.X, Is.EqualTo(20));
        Assert.That(info.Position.Y, Is.EqualTo(25));
    }

    [Test]
    public void HandlePointerLeftButtonReleased_TransitionsToSelectionPreviewState()
    {
        var cursorArea = HalfBoxArea.Create(_startPosition, new PictureSize(8, 8));

        var nextState = _state.HandlePointerLeftButtonReleased(cursorArea, new Position(20, 20), null, null);

        Assert.That(nextState, Is.InstanceOf<SelectionPreviewState>());
    }

    [Test]
    public void GetCursor_ReturnsMove()
    {
        var cursor = _state.GetCursor(new Position(10, 10));
        Assert.That(cursor, Is.EqualTo(SelectionCursor.Move));
    }

    [Test]
    public void GetSelectingArea_ReturnsShiftedArea()
    {
        var cursorArea = HalfBoxArea.Create(_startPosition, new PictureSize(8, 8));
        var canvasSize = new PictureSize(100, 100);

        _state.HandlePointerMoved(cursorArea, true, new Position(20, 20), false, canvasSize);

        var area = _state.GetSelectingArea();
        Assert.That(area, Is.Not.Null);
        // Original (5, 5) + offset (10, 10) = (15, 15)
        Assert.That(area!.Value.X, Is.EqualTo(15));
        Assert.That(area.Value.Y, Is.EqualTo(15));
    }

    [Test]
    public void GetOriginalArea_ReturnsConfiguredOriginalArea()
    {
        Assert.That(_state.GetOriginalArea(), Is.EqualTo(_originalArea));
    }

    [Test]
    public void Commit_WithDirectImageBlender_CommitsPreview()
    {
        var session = new DrawingSession(Picture.CreateEmpty(new PictureSize(50, 50)));
        var blender = new Eede.Domain.ImageEditing.Blending.DirectImageBlender();
        var bg = new Eede.Domain.Palettes.ArgbColor(255, 255, 255, 255);

        var resultSession = _state.Commit(session, blender, bg);

        Assert.That(resultSession, Is.Not.Null);
        Assert.That(resultSession.CurrentPreviewContent, Is.Null);
        Assert.That(resultSession.CanUndo(), Is.True);
    }

    [Test]
    public void Commit_WithAlphaImageBlender_AppliesTransparencyAndCommits()
    {
        var session = new DrawingSession(Picture.CreateEmpty(new PictureSize(50, 50)));
        var blender = new Eede.Domain.ImageEditing.Blending.AlphaImageBlender();
        var bg = new Eede.Domain.Palettes.ArgbColor(255, 0, 0, 0);

        var resultSession = _state.Commit(session, blender, bg);

        Assert.That(resultSession, Is.Not.Null);
        Assert.That(resultSession.CurrentPreviewContent, Is.Null);
        Assert.That(resultSession.CanUndo(), Is.True);
    }

    [Test]
    public void Cancel_CancelsDrawingAndClearsPreview()
    {
        var session = new DrawingSession(Picture.CreateEmpty(new PictureSize(50, 50)));

        var resultSession = _state.Cancel(session);

        Assert.That(resultSession, Is.Not.Null);
        Assert.That(resultSession.CurrentPreviewContent, Is.Null);
    }
}

