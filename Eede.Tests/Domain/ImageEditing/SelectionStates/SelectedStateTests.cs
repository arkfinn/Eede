using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.SelectionStates;

[TestFixture]
public class SelectedStateTests
{
    private Selection _selection;
    private SelectedState _state;
    private Picture _samplePicture;

    [SetUp]
    public void SetUp()
    {
        var area = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
        _selection = new Selection(area);
        _state = new SelectedState(_selection);
        _samplePicture = Picture.Create(new PictureSize(40, 40), new byte[40 * 40 * 4]);
    }

    [Test]
    public void HandlePointerLeftButtonPressed_OnHandle_TransitionsToResizingState()
    {
        var cursorArea = HalfBoxArea.Create(new Position(10, 10), new PictureSize(8, 8));
        // Click on top-left corner (10, 10)
        var nextState = _state.HandlePointerLeftButtonPressed(cursorArea, new Position(10, 10), null, () => _samplePicture, null, 8);

        Assert.That(nextState, Is.InstanceOf<ResizingState>());
    }

    [Test]
    public void HandlePointerLeftButtonPressed_InsideSelection_TransitionsToDraggingState()
    {
        var cursorArea = HalfBoxArea.Create(new Position(20, 20), new PictureSize(8, 8));
        // Click inside the selection (20, 20) away from handle
        var nextState = _state.HandlePointerLeftButtonPressed(cursorArea, new Position(20, 20), null, () => _samplePicture, null, 2);

        Assert.That(nextState, Is.InstanceOf<DraggingState>());
    }

    [Test]
    public void HandlePointerLeftButtonPressed_OutsideSelection_TransitionsToNormalCursorState()
    {
        var cursorArea = HalfBoxArea.Create(new Position(0, 0), new PictureSize(8, 8));
        var nextState = _state.HandlePointerLeftButtonPressed(cursorArea, new Position(0, 0), null, () => _samplePicture, null, 2);

        Assert.That(nextState, Is.InstanceOf<NormalCursorState>());
    }

    [Test]
    public void HandlePointerRightButtonPressed_TransitionsToNormalCursorState()
    {
        var cursorArea = HalfBoxArea.Create(new Position(20, 20), new PictureSize(8, 8));
        var (nextState, _) = _state.HandlePointerRightButtonPressed(cursorArea, new Position(20, 20), new PictureSize(8, 8), null);

        Assert.That(nextState, Is.InstanceOf<NormalCursorState>());
    }

    [Test]
    public void GetCursor_ReturnsMove_WhenInsideSelection()
    {
        var cursor = _state.GetCursor(new Position(20, 20), 2);
        Assert.That(cursor, Is.EqualTo(SelectionCursor.Move));
    }

    [Test]
    public void GetCursor_ReturnsDefault_WhenOutsideSelection()
    {
        var cursor = _state.GetCursor(new Position(0, 0), 2);
        Assert.That(cursor, Is.EqualTo(SelectionCursor.Default));
    }

    [Test]
    public void GetSelectingArea_ReturnsSelectionArea()
    {
        var area = _state.GetSelectingArea();
        Assert.That(area, Is.EqualTo(_selection.Area));
    }
}
