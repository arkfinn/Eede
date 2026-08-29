using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Tests.Selections
{
    [TestFixture]
    public class SelectionHandleDetectorTests
    {
        [TestCase(10, 20, SelectionHandle.TopLeft)]
        [TestCase(110, 20, SelectionHandle.TopRight)]
        [TestCase(10, 70, SelectionHandle.BottomLeft)]
        [TestCase(110, 70, SelectionHandle.BottomRight)]
        public void Detect_Corners_ReturnsExpectedHandle(int x, int y, SelectionHandle expected)
        {
            var area = new PictureArea(new Position(10, 20), new PictureSize(100, 50));
            var position = new Position(x, y);
            var handleSize = 10;

            var result = SelectionHandleDetector.Detect(area, position, handleSize);

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(60, 20, SelectionHandle.Top)]
        [TestCase(60, 70, SelectionHandle.Bottom)]
        [TestCase(10, 45, SelectionHandle.Left)]
        [TestCase(110, 45, SelectionHandle.Right)]
        public void Detect_Edges_ReturnsExpectedHandle(int x, int y, SelectionHandle expected)
        {
            var area = new PictureArea(new Position(10, 20), new PictureSize(100, 50));
            var position = new Position(x, y);
            var handleSize = 10;

            var result = SelectionHandleDetector.Detect(area, position, handleSize);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Detect_Inside_ReturnsNull()
        {
            var area = new PictureArea(new Position(10, 20), new PictureSize(100, 50));
            var position = new Position(60, 45); // Middle
            var handleSize = 10;

            var result = SelectionHandleDetector.Detect(area, position, handleSize);

            Assert.That(result, Is.Null);
        }

        [TestCase(5, 5, SelectionHandle.TopLeft)]
        [TestCase(15, 15, SelectionHandle.TopLeft)]
        [TestCase(16, 16, null)]
        public void Detect_SmallArea_AdjustsHandleSize_ReturnsExpected(int x, int y, SelectionHandle? expected)
        {
            // minDimension = 30.
            // adjustedHandleSize = 12.
            // 24 > 20 => adjustedHandleSize = Math.Max(1, 30 / 3) = 10.
            // tolerance = 10 / 2 = 5.
            // TopLeft targetX/Y is 10/10. Hits if X/Y in [5, 15].
            var area = new PictureArea(new Position(10, 10), new PictureSize(30, 30));
            var position = new Position(x, y);
            var handleSize = 12;

            var result = SelectionHandleDetector.Detect(area, position, handleSize);

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
