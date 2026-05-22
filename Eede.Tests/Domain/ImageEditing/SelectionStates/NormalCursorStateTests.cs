using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.SelectionStates;

[TestFixture]
public class NormalCursorStateTests
{
    [Test]
    public void HandlePointerRightButtonPressed_TransitionsToRegionSelectingState()
    {
        // Arrange
        var initialCursorArea = HalfBoxArea.Create(new Position(10, 10), new PictureSize(16, 16));
        var state = new NormalCursorState(initialCursorArea);

        var nowPosition = new Position(12, 12);
        var minCursorSize = new PictureSize(8, 8);

        // Act
        var (newState, newCursorArea) = state.HandlePointerRightButtonPressed(
            initialCursorArea,
            nowPosition,
            minCursorSize,
            null);

        // Assert
        Assert.That(newState, Is.InstanceOf<RegionSelectingState>());
        Assert.That(newCursorArea, Is.EqualTo(initialCursorArea));
    }

    [Test]
    [TestCase(0, 0, true, 100, 100)]
    [TestCase(50, 50, true, 100, 100)]
    [TestCase(100, 100, false, 100, 100)] // Boundary logic in PictureSize.Contains makes 100 out of bounds usually
    [TestCase(101, 101, false, 100, 100)] // Outside boundary
    [TestCase(-1, -1, false, 100, 100)] // Outside boundary
    public void HandlePointerMoved_ReturnsExpectedVisibilityAndUpdatesCursor(int posX, int posY, bool expectedVisibility, int canvasWidth, int canvasHeight)
    {
        // Arrange
        var initialCursorArea = HalfBoxArea.Create(new Position(10, 10), new PictureSize(16, 16));
        var state = new NormalCursorState(initialCursorArea);

        var nowPosition = new Position(posX, posY);
        var canvasSize = new PictureSize(canvasWidth, canvasHeight);

        // Act
        var (isVisible, newCursorArea) = state.HandlePointerMoved(
            initialCursorArea,
            true, // visibleCursor param is ignored in method implementation
            nowPosition,
            false,
            canvasSize);

        // Assert
        Assert.That(isVisible, Is.EqualTo(expectedVisibility));

        var expectedArea = initialCursorArea.Move(nowPosition);
        Assert.That(newCursorArea.RealPosition.X, Is.EqualTo(expectedArea.RealPosition.X));
        Assert.That(newCursorArea.RealPosition.Y, Is.EqualTo(expectedArea.RealPosition.Y));
        Assert.That(newCursorArea.BoxSize.Width, Is.EqualTo(expectedArea.BoxSize.Width));
        Assert.That(newCursorArea.BoxSize.Height, Is.EqualTo(expectedArea.BoxSize.Height));
    }

    [Test]
    public void HandlePointerMoved_InsideCanvas_ReturnsVisibleCursorAndMovedArea()
    {
        // Arrange
        var initialArea = HalfBoxArea.Create(new Position(0, 0), new PictureSize(10, 10));
        var state = new NormalCursorState(initialArea);
        var nowPosition = new Position(5, 5);
        var canvasSize = new PictureSize(20, 20);

        // Act
        var (visibleCursor, newArea) = state.HandlePointerMoved(initialArea, false, nowPosition, false, canvasSize);

        // Assert
        Assert.That(visibleCursor, Is.True, "Cursor should be visible when inside canvas");
        Assert.That(newArea.RealPosition, Is.EqualTo(nowPosition), "Cursor area should be moved to new position");
        Assert.That(newArea.BoxSize, Is.EqualTo(new PictureSize(10, 10)), "Box size should remain the same");
    }

    [Test]
    public void HandlePointerMoved_OutsideCanvas_ReturnsInvisibleCursorAndMovedArea()
    {
        // Arrange
        var initialArea = HalfBoxArea.Create(new Position(0, 0), new PictureSize(10, 10));
        var state = new NormalCursorState(initialArea);
        var nowPosition = new Position(25, 25);
        var canvasSize = new PictureSize(20, 20);

        // Act
        var (visibleCursor, newArea) = state.HandlePointerMoved(initialArea, true, nowPosition, false, canvasSize);

        // Assert
        Assert.That(visibleCursor, Is.False, "Cursor should not be visible when outside canvas");
        Assert.That(newArea.RealPosition, Is.EqualTo(nowPosition), "Cursor area should be moved to new position");
        Assert.That(newArea.BoxSize, Is.EqualTo(new PictureSize(10, 10)), "Box size should remain the same");
    }

    [Test]
    public void HandlePointerRightButtonReleased_ReturnsSameStateAndArea()
    {
        // Arrange
        var initialArea = HalfBoxArea.Create(new Position(5, 5), new PictureSize(10, 10));
        var state = new NormalCursorState(initialArea);

        // Act
        var (newState, newArea) = state.HandlePointerRightButtonReleased(initialArea, null);

        // Assert
        Assert.That(newState, Is.SameAs(state), "Should return the same state instance");
        Assert.That(newArea.RealPosition, Is.EqualTo(initialArea.RealPosition), "Position should remain unchanged");
        Assert.That(newArea.BoxSize, Is.EqualTo(initialArea.BoxSize), "Box size should remain unchanged");
    }
}
