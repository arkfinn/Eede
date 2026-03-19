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
}
