using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.SelectionStates
{
    [TestFixture]
    public class NormalCursorStateTests
    {
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
}
