using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.ImageEditing.SelectionStates;

[TestFixture]
public class SelectionPreviewStateTests
{
    private Picture _samplePicture;
    private SelectionPreviewInfo _previewInfo;
    private SelectionPreviewState _state;

    [SetUp]
    public void SetUp()
    {
        _samplePicture = Picture.Create(new PictureSize(20, 20), new byte[20 * 20 * 4]);
        _previewInfo = new SelectionPreviewInfo(_samplePicture, new Position(10, 10), SelectionPreviewType.CutAndMove, null, _samplePicture);
        _state = new SelectionPreviewState(_previewInfo);
    }

    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenInfoIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new SelectionPreviewState(null!));
    }

    [Test]
    public void HandlePointerLeftButtonPressed_OnHandle_TransitionsToResizingState()
    {
        var cursorArea = HalfBoxArea.Create(new Position(10, 10), new PictureSize(8, 8));
        // Click on top-left corner
        var nextState = _state.HandlePointerLeftButtonPressed(cursorArea, new Position(10, 10), null, () => _samplePicture, null, 8);

        Assert.That(nextState, Is.InstanceOf<ResizingState>());
    }

    [Test]
    public void HandlePointerLeftButtonPressed_InsidePreview_TransitionsToDraggingState()
    {
        var cursorArea = HalfBoxArea.Create(new Position(20, 20), new PictureSize(8, 8));
        // Click inside away from handles
        var nextState = _state.HandlePointerLeftButtonPressed(cursorArea, new Position(20, 20), null, () => _samplePicture, null, 2);

        Assert.That(nextState, Is.InstanceOf<DraggingState>());
    }

    [Test]
    public void HandlePointerLeftButtonPressed_OutsidePreview_TransitionsToNormalCursorState()
    {
        var cursorArea = HalfBoxArea.Create(new Position(0, 0), new PictureSize(8, 8));
        var nextState = _state.HandlePointerLeftButtonPressed(cursorArea, new Position(0, 0), null, () => _samplePicture, null, 2);

        Assert.That(nextState, Is.InstanceOf<NormalCursorState>());
    }

    [Test]
    public void HandlePointerRightButtonPressed_TransitionsToNormalCursorState()
    {
        var cursorArea = HalfBoxArea.Create(new Position(10, 10), new PictureSize(8, 8));
        var (nextState, _) = _state.HandlePointerRightButtonPressed(cursorArea, new Position(10, 10), new PictureSize(8, 8), null);

        Assert.That(nextState, Is.InstanceOf<NormalCursorState>());
    }

    [Test]
    public void GetSelectingArea_ReturnsPreviewArea()
    {
        var area = _state.GetSelectingArea();
        Assert.That(area, Is.Not.Null);
        Assert.That(area!.Value.X, Is.EqualTo(10));
        Assert.That(area.Value.Y, Is.EqualTo(10));
        Assert.That(area.Value.Width, Is.EqualTo(20));
        Assert.That(area.Value.Height, Is.EqualTo(20));
    }
}
