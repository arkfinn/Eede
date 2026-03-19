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
        public void HandlePointerRightButtonPressed_TransitionsToRegionSelectingState()
        {
            var cursorPosition = new Position(10, 10);
            var cursorSize = new PictureSize(16, 16);
            var cursorArea = HalfBoxArea.Create(cursorPosition, cursorSize);
            var state = new NormalCursorState(cursorArea);

            var nowPosition = new Position(20, 20);
            var minCursorSize = new PictureSize(8, 8);

            var (newState, returnedCursorArea) = state.HandlePointerRightButtonPressed(cursorArea, nowPosition, minCursorSize, null);

            Assert.That(newState, Is.InstanceOf<RegionSelectingState>());
            Assert.That(returnedCursorArea.RealPosition, Is.EqualTo(cursorArea.RealPosition));
            Assert.That(returnedCursorArea.BoxSize, Is.EqualTo(cursorArea.BoxSize));
        }

        [Test]
        public void HandlePointerMoved_UpdatesCursorAreaAndVisibility()
        {
            var cursorPosition = new Position(10, 10);
            var cursorSize = new PictureSize(16, 16);
            var cursorArea = HalfBoxArea.Create(cursorPosition, cursorSize);
            var state = new NormalCursorState(cursorArea);

            var nowPosition = new Position(30, 30);
            var canvasSize = new PictureSize(100, 100);

            var (visibleCursor, newCursorArea) = state.HandlePointerMoved(cursorArea, true, nowPosition, false, canvasSize);

            Assert.That(visibleCursor, Is.True);
            // HalfBoxArea.Move will snap to grid. gridSize = boxSize / 2 = (8, 8).
            // Snapped(30, 8) = 24. RealPosition will be (24, 24).
            Assert.That(newCursorArea.RealPosition, Is.EqualTo(new Position(24, 24)));
            Assert.That(newCursorArea.BoxSize, Is.EqualTo(cursorSize));
        }

        [Test]
        public void HandlePointerMoved_OutsideCanvas_HidesCursor()
        {
            var cursorPosition = new Position(10, 10);
            var cursorSize = new PictureSize(16, 16);
            var cursorArea = HalfBoxArea.Create(cursorPosition, cursorSize);
            var state = new NormalCursorState(cursorArea);

            var nowPosition = new Position(150, 150);
            var canvasSize = new PictureSize(100, 100);

            var (visibleCursor, newCursorArea) = state.HandlePointerMoved(cursorArea, true, nowPosition, false, canvasSize);

            Assert.That(visibleCursor, Is.False);
            // Snapped(150, 8) = 144
            Assert.That(newCursorArea.RealPosition, Is.EqualTo(new Position(144, 144)));
            Assert.That(newCursorArea.BoxSize, Is.EqualTo(cursorSize));
        }
    }
}
