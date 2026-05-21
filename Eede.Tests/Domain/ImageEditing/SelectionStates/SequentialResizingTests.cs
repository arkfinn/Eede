using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.SelectionStates
{
    [TestFixture]
    public class SequentialResizingTests
    {
        [Test]
        public void SequentialResize_ShouldWork()
        {
            // 1. Initial Selection at (10, 10) size 20x20
            var area = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var selection = new Selection(area);
            var state = new SelectedState(selection);
            var pixels = Picture.CreateEmpty(new PictureSize(20, 20));
            var cursorArea = HalfBoxArea.Create(new Position(0, 0), new PictureSize(16, 16));

            // 2. Start First Resize at BottomRight (30, 30)
            var resizingState = state.HandlePointerLeftButtonPressed(cursorArea, new Position(30, 30), null, () => pixels, null) as ResizingState;
            Assert.That(resizingState, Is.Not.Null, "Should transition to ResizingState");

            // 3. Move to (40, 40) -> Size becomes 30x30
            resizingState.HandlePointerMoved(cursorArea, true, new Position(40, 40), false, new PictureSize(100, 100));

            // 4. Release -> Should transition to SelectionPreviewState
            var previewState = resizingState.HandlePointerLeftButtonReleased(cursorArea, new Position(40, 40), null, null) as SelectionPreviewState;
            Assert.That(previewState, Is.Not.Null, "Should transition to SelectionPreviewState");
            Assert.That(previewState.GetSelectingArea().Value.Size.Width, Is.EqualTo(30), "Size should be 30");

            // 5. Start Second Resize at NEW BottomRight (40, 40)
            var secondResizingState = previewState.HandlePointerLeftButtonPressed(cursorArea, new Position(40, 40), null, () => pixels, null) as ResizingState;
            Assert.That(secondResizingState, Is.Not.Null, "Should transition back to ResizingState for the second time");

            // 6. Move to (50, 50) -> Size becomes 40x40
            secondResizingState.HandlePointerMoved(cursorArea, true, new Position(50, 50), false, new PictureSize(100, 100));
            var finalPreview = secondResizingState.HandlePointerLeftButtonReleased(cursorArea, new Position(50, 50), null, null) as SelectionPreviewState;
            Assert.That(finalPreview.GetSelectingArea().Value.Size.Width, Is.EqualTo(40), "Final size should be 40");
        }
    }
}
