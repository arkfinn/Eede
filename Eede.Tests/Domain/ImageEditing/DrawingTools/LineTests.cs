using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools
{
    [TestFixture]
    public class LineTests
    {
        private Picture _src;
        private DrawingBuffer _buffer;
        private PenStyle _penStyle;
        private Line _tool;
        private CoordinateHistory _startPos;

        [SetUp]
        public void Setup()
        {
            _src = PictureHelper.ReadBitmap(@"ImageEditing\DrawingTools\test\base.png");
            _buffer = new DrawingBuffer(_src);
            _penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1);
            _tool = new Line();
            _startPos = new CoordinateHistory(new CanvasCoordinate(10, 10));
        }

        [Test]
        public void DrawTest_NormalLine()
        {
            // Act
            DrawingBuffer startBuffer = _tool.DrawStart(_buffer, _penStyle, _startPos, false);
            CoordinateHistory endPos = _startPos.Update(new CanvasCoordinate(20, 15));
            var result = _tool.DrawEnd(startBuffer, _penStyle, endPos, false);

            var bounds = result.AffectedArea.GetBoundingBox();

            // Assert
            Assert.That(bounds.Position.X, Is.EqualTo(10));
            Assert.That(bounds.Position.Y, Is.EqualTo(10));
            Assert.That(bounds.Size.Width, Is.EqualTo(11));
            Assert.That(bounds.Size.Height, Is.EqualTo(6));
        }

        [Test]
        public void DrawTest_ShiftHorizontal()
        {
            // Act
            DrawingBuffer startBuffer = _tool.DrawStart(_buffer, _penStyle, _startPos, true);
            // Move to (20, 11). Since it's shift mode, it should snap to horizontal (20, 10).
            CoordinateHistory endPos = _startPos.Update(new CanvasCoordinate(20, 11));
            var result = _tool.DrawEnd(startBuffer, _penStyle, endPos, true);

            var bounds = result.AffectedArea.GetBoundingBox();

            // Assert
            // Snapped to (20, 10). Area should be X:10, Y:10, Width:11, Height:1.
            Assert.That(bounds.Position.X, Is.EqualTo(10));
            Assert.That(bounds.Position.Y, Is.EqualTo(10));
            Assert.That(bounds.Size.Width, Is.EqualTo(11));
            Assert.That(bounds.Size.Height, Is.EqualTo(1));
        }

        [Test]
        public void DrawTest_ShiftDiagonal()
        {
            // Act
            DrawingBuffer startBuffer = _tool.DrawStart(_buffer, _penStyle, _startPos, true);
            // Move to (20, 18). Should snap to 45 degrees, which is (20, 20).
            CoordinateHistory endPos = _startPos.Update(new CanvasCoordinate(20, 18));
            var result = _tool.DrawEnd(startBuffer, _penStyle, endPos, true);

            var bounds = result.AffectedArea.GetBoundingBox();

            // Assert
            // Snapped to (20, 20). Area should be X:10, Y:10, Width:11, Height:11.
            Assert.That(bounds.Position.X, Is.EqualTo(10));
            Assert.That(bounds.Position.Y, Is.EqualTo(10));
            Assert.That(bounds.Size.Width, Is.EqualTo(11));
            Assert.That(bounds.Size.Height, Is.EqualTo(11));
        }

        [Test]
        public void DrawTest_DrawStartEndAreaCombined()
        {
            // Act
            DrawingBuffer startBuffer = _tool.DrawStart(_buffer, _penStyle, _startPos, false);
            // Move slightly backwards and up to (8, 8)
            CoordinateHistory endPos = _startPos.Update(new CanvasCoordinate(8, 8));
            var result = _tool.DrawEnd(startBuffer, _penStyle, endPos, false);

            var bounds = result.AffectedArea.GetBoundingBox();

            // Assert
            // Start area was (10, 10, 1, 1). End line area is from (10, 10) to (8, 8).
            // Line area should be X:8, Y:8, Width:3, Height:3.
            // Combined with Start area, it should still be X:8, Y:8, Width:3, Height:3.
            Assert.That(bounds.Position.X, Is.EqualTo(8));
            Assert.That(bounds.Position.Y, Is.EqualTo(8));
            Assert.That(bounds.Size.Width, Is.EqualTo(3));
            Assert.That(bounds.Size.Height, Is.EqualTo(3));
        }
    }
}
