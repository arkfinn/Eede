using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.SelectionStates
{
    [TestFixture]
    public class ResizingStateTests
    {
        [Test]
        public void GetSelectionPreviewInfo_ReturnsResizedImage()
        {
            var originalPixels = Picture.CreateEmpty(new PictureSize(10, 10));
            var originalArea = new PictureArea(new Position(0, 0), new PictureSize(10, 10));
            var startPos = new Position(10, 10); // Bottom Right Corner
            var resampler = new NearestNeighborResampler();

            var state = new ResizingState(
                originalPixels,
                originalArea,
                startPos,
                SelectionHandle.BottomRight,
                resampler
            );

            // Drag to (20, 20) -> Size becomes 20x20
            state.HandlePointerMoved(HalfBoxArea.Create(startPos, new PictureSize(2, 2)), true, new Position(20, 20), false, new PictureSize(100, 100));

            var info = state.GetSelectionPreviewInfo();

            Assert.That(info, Is.Not.Null);
            Assert.That(info.Position.X, Is.EqualTo(0));
            Assert.That(info.Position.Y, Is.EqualTo(0));
            Assert.That(info.Pixels.Width, Is.EqualTo(20));
            Assert.That(info.Pixels.Height, Is.EqualTo(20));
        }
    }
}
