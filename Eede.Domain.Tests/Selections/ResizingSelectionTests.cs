using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.Selections
{
    [TestFixture]
    public class ResizingSelectionTests
    {
        [Test]
        public void Resize_BottomRight_IncreasesSize()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var resizer = new ResizingSelection(original, SelectionHandle.BottomRight);
            
            var startPos = new Position(30, 30); // BottomRight corner of original
            var currentPos = new Position(40, 50); // Dragged by +10, +20

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.X, Is.EqualTo(10));
                Assert.That(result.Y, Is.EqualTo(10));
                Assert.That(result.Width, Is.EqualTo(30)); // 20 + 10
                Assert.That(result.Height, Is.EqualTo(40)); // 20 + 20
            });
        }

        [Test]
        public void Resize_TopLeft_MovesPositionAndResizes()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var resizer = new ResizingSelection(original, SelectionHandle.TopLeft);
            
            var startPos = new Position(10, 10);
            var currentPos = new Position(5, 5); // Dragged by -5, -5

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.X, Is.EqualTo(5));
                Assert.That(result.Y, Is.EqualTo(5));
                Assert.That(result.Width, Is.EqualTo(25)); // 20 + 5
                Assert.That(result.Height, Is.EqualTo(25)); // 20 + 5
            });
        }

        [Test]
        public void Resize_TopRight_MovesPositionAndResizes()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var resizer = new ResizingSelection(original, SelectionHandle.TopRight);

            var startPos = new Position(30, 10); // TopRight corner
            var currentPos = new Position(35, 5); // Dragged by +5, -5

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.X, Is.EqualTo(10));
                Assert.That(result.Y, Is.EqualTo(5));
                Assert.That(result.Width, Is.EqualTo(25)); // 20 + 5
                Assert.That(result.Height, Is.EqualTo(25)); // 20 + 5
            });
        }

        [Test]
        public void Resize_BottomLeft_MovesPositionAndResizes()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var resizer = new ResizingSelection(original, SelectionHandle.BottomLeft);

            var startPos = new Position(10, 30); // BottomLeft corner
            var currentPos = new Position(5, 35); // Dragged by -5, +5

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.X, Is.EqualTo(5));
                Assert.That(result.Y, Is.EqualTo(10));
                Assert.That(result.Width, Is.EqualTo(25)); // 20 + 5
                Assert.That(result.Height, Is.EqualTo(25)); // 20 + 5
            });
        }

        [Test]
        public void Resize_MinimumSizeIs1px()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var resizer = new ResizingSelection(original, SelectionHandle.BottomRight);
            
            var startPos = new Position(30, 30);
            var currentPos = new Position(10, 10); // Try to shrink to 0 size

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(1));
                Assert.That(result.Height, Is.EqualTo(1));
            });
        }

        [Test]
        public void Resize_KeepAspectRatio_Square()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20)); // 1:1 ratio
            var resizer = new ResizingSelection(original, SelectionHandle.BottomRight);
            
            var startPos = new Position(30, 30);
            var currentPos = new Position(40, 50); // Dragged to make width 30, height 40
            
            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: true);

            Assert.That(result.Width, Is.EqualTo(result.Height));
        }

        [Test]
        public void Resize_TopRight_KeepAspectRatio_Square()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20)); // 1:1 ratio
            var resizer = new ResizingSelection(original, SelectionHandle.TopRight);

            var startPos = new Position(30, 10);
            var currentPos = new Position(40, 0); // Dragged to make width 30, height 30

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: true);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(result.Height));
                Assert.That(result.X, Is.EqualTo(10));
                Assert.That(result.Y + result.Height, Is.EqualTo(30)); // bottom (Y + Height) shouldn't change
            });
        }

        [Test]
        public void Resize_BottomLeft_KeepAspectRatio_Square()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20)); // 1:1 ratio
            var resizer = new ResizingSelection(original, SelectionHandle.BottomLeft);

            var startPos = new Position(10, 30);
            var currentPos = new Position(0, 40); // Dragged to make width 30, height 30

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: true);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(result.Height));
                Assert.That(result.Y, Is.EqualTo(10));
                Assert.That(result.X + result.Width, Is.EqualTo(30)); // right (X + Width) shouldn't change
            });
        }

        [Test]
        public void Resize_BottomRight_VerticalDrag_ExpandsBoth()
        {
            var original = new PictureArea(new Position(0, 0), new PictureSize(100, 100));
            var resizer = new ResizingSelection(original, SelectionHandle.BottomRight);

            var startPos = new Position(100, 100);
            var currentPos = new Position(100, 120); // Pure vertical drag

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: true);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(120));
                Assert.That(result.Height, Is.EqualTo(120));
            });
        }

        [Test]
        public void Resize_BottomRight_Shrink_Works()
        {
            var original = new PictureArea(new Position(0, 0), new PictureSize(100, 100));
            var resizer = new ResizingSelection(original, SelectionHandle.BottomRight);

            var startPos = new Position(100, 100);
            var currentPos = new Position(80, 90); // Shrink, more on width

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: true);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(80));
                Assert.That(result.Height, Is.EqualTo(80));
            });
        }
    }
}
