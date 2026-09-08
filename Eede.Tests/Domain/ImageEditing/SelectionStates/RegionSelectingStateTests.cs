using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.SelectionStates;

[TestFixture]
public class RegionSelectingStateTests
{
    [Test]
    public void HandlePointerMoved_UpdatesCursorAreaAndVisibility()
    {
        var start = new Position(10, 10);
        var state = new RegionSelectingState(start, start, new PictureSize(8, 8), new PictureSize(16, 16));
        var cursorArea = HalfBoxArea.Create(start, new PictureSize(16, 16));
        var canvasSize = new PictureSize(100, 100);

        var (isVisible, newArea) = state.HandlePointerMoved(cursorArea, true, new Position(30, 40), false, canvasSize);

        Assert.That(isVisible, Is.True);
        var expected = cursorArea.Move(new Position(30, 40));
        Assert.That(newArea.RealPosition.X, Is.EqualTo(expected.RealPosition.X));
        Assert.That(newArea.RealPosition.Y, Is.EqualTo(expected.RealPosition.Y));
    }

    [Test]
    public void GetSelectingArea_WithShift_ConstrainsToSquare()
    {
        var start = new Position(10, 10);
        var state = new RegionSelectingState(start, start, new PictureSize(4, 4), new PictureSize(4, 4));
        var cursorArea = HalfBoxArea.Create(start, new PictureSize(8, 8));
        var canvasSize = new PictureSize(100, 100);

        // Move with shift = true, deltaX = 20, deltaY = 10 => should constrain to max delta = 20x20
        state.HandlePointerMoved(cursorArea, true, new Position(30, 20), true, canvasSize);

        var area = state.GetSelectingArea();
        Assert.That(area, Is.Not.Null);
        Assert.That(area!.Value.Width, Is.EqualTo(area.Value.Height));
    }

    [Test]
    public void HandlePointerRightButtonReleased_TransitionsToSelectedState_WhenAreaValid()
    {
        var start = new Position(10, 10);
        var state = new RegionSelectingState(start, start, new PictureSize(4, 4), new PictureSize(4, 4));
        var cursorArea = HalfBoxArea.Create(start, new PictureSize(8, 8));
        var canvasSize = new PictureSize(100, 100);

        state.HandlePointerMoved(cursorArea, true, new Position(50, 50), false, canvasSize);

        var (newState, newArea) = state.HandlePointerRightButtonReleased(cursorArea, null);

        Assert.That(newState, Is.InstanceOf<SelectedState>());
    }

    [Test]
    public void HandlePointerLeftButtonPressedAndReleased_ReturnsSelf()
    {
        var start = new Position(10, 10);
        var state = new RegionSelectingState(start, start, new PictureSize(4, 4), new PictureSize(4, 4));
        var cursorArea = HalfBoxArea.Create(start, new PictureSize(8, 8));

        var pressResult = state.HandlePointerLeftButtonPressed(cursorArea, start, null, () => null!, null);
        var releaseResult = state.HandlePointerLeftButtonReleased(cursorArea, start, null, null);

        Assert.That(pressResult, Is.SameAs(state));
        Assert.That(releaseResult, Is.SameAs(state));
    }
}
