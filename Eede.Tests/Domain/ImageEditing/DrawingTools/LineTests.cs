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

        [Test]
        public void DrawingTest_UpdatesDrawingBufferPreview()
        {
            // Act
            DrawingBuffer startBuffer = _tool.DrawStart(_buffer, _penStyle, _startPos, false);
            CoordinateHistory movePos = _startPos.Update(new CanvasCoordinate(15, 10));
            DrawingBuffer drawingBuffer = _tool.Drawing(startBuffer, _penStyle, movePos, false);

            // Assert: drawingBuffer の Fetch() はプレビュー線が反映され、Previous は変更されていない
            Assert.Multiple(() =>
            {
                Assert.That(drawingBuffer.Fetch(), Is.Not.EqualTo(_buffer.Previous));
                Assert.That(drawingBuffer.Previous, Is.EqualTo(_buffer.Previous));
            });
        }

        [Test]
        public void DrawEnd_WithoutPreviousDrawing_UsesResultAreaDirectly()
        {
            // DrawStart や Drawing を一度も呼ばずに直接 DrawEnd を呼ぶ (AffectedArea は null)
            CoordinateHistory pos = new(new CanvasCoordinate(5, 5));
            var result = _tool.DrawEnd(_buffer, _penStyle, pos, false);

            var bounds = result.AffectedArea.GetBoundingBox();
            Assert.Multiple(() =>
            {
                Assert.That(bounds.Position.X, Is.EqualTo(5));
                Assert.That(bounds.Position.Y, Is.EqualTo(5));
                Assert.That(bounds.Size.Width, Is.EqualTo(1));
                Assert.That(bounds.Size.Height, Is.EqualTo(1));
            });
        }

        [Test]
        public void DrawEnd_WhenShrunkAfterExpansion_CombinesAreas()
        {
            // Act: (10, 10) から開始
            DrawingBuffer startBuffer = _tool.DrawStart(_buffer, _penStyle, _startPos, false);
            // (20, 20) へドラッグ拡大 (10, 10, 11, 11)
            CoordinateHistory movePos = _startPos.Update(new CanvasCoordinate(20, 20));
            DrawingBuffer drawingBuffer = _tool.Drawing(startBuffer, _penStyle, movePos, false);
            // (10, 12) へ縮小して終了 (10, 10, 1, 3)
            CoordinateHistory endPos = movePos.Update(new CanvasCoordinate(10, 12));
            var result = _tool.DrawEnd(drawingBuffer, _penStyle, endPos, false);

            var bounds = result.AffectedArea.GetBoundingBox();

            // Assert: 直前の (10, 10, 11, 11) と終了時の (10, 10, 1, 3) が Combine されて (10, 10, 11, 11) になること
            Assert.Multiple(() =>
            {
                Assert.That(bounds.Position.X, Is.EqualTo(10));
                Assert.That(bounds.Position.Y, Is.EqualTo(10));
                Assert.That(bounds.Size.Width, Is.EqualTo(11));
                Assert.That(bounds.Size.Height, Is.EqualTo(11));
            });
        }

        [TestCase(10, 24, 10, 20, 11, 1, Description = "左やや下方向 (deg=15 スナップ)")]
        [TestCase(30, 21, 20, 20, 11, 1, Description = "右方向 (水平)")]
        [TestCase(30, 29, 20, 20, 11, 11, Description = "右下方向 (45度)")]
        [TestCase(21, 30, 20, 20, 1, 11, Description = "下方向 (垂直)")]
        [TestCase(11, 29, 11, 20, 10, 10, Description = "左下方向 (135度)")]
        [TestCase(10, 21, 10, 20, 11, 1, Description = "左方向 (水平)")]
        [TestCase(11, 11, 11, 11, 10, 10, Description = "左上方向 (225度)")]
        [TestCase(21, 10, 20, 10, 1, 11, Description = "上方向 (垂直)")]
        [TestCase(29, 11, 20, 11, 10, 10, Description = "右上方向 (315度)")]
        public void DrawTest_Shift_AllDirections(int targetX, int targetY, int expectedX, int expectedY, int expectedW, int expectedH)
        {
            // Arrange: (20, 20) を始点とする
            CoordinateHistory centerPos = new(new CanvasCoordinate(20, 20));

            // Act
            DrawingBuffer startBuffer = _tool.DrawStart(_buffer, _penStyle, centerPos, true);
            CoordinateHistory endPos = centerPos.Update(new CanvasCoordinate(targetX, targetY));
            var result = _tool.DrawEnd(startBuffer, _penStyle, endPos, true);

            var bounds = result.AffectedArea.GetBoundingBox();

            // Assert: 8 方向すべてのスナップ位置とバウンディングボックスを検証
            Assert.Multiple(() =>
            {
                Assert.That(bounds.Position.X, Is.EqualTo(expectedX), "X mismatch");
                Assert.That(bounds.Position.Y, Is.EqualTo(expectedY), "Y mismatch");
                Assert.That(bounds.Size.Width, Is.EqualTo(expectedW), "Width mismatch");
                Assert.That(bounds.Size.Height, Is.EqualTo(expectedH), "Height mismatch");
            });
        }
    }
}
