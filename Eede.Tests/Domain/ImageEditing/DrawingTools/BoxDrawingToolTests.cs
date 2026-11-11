using System;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools
{
    [TestFixture]
    public class BoxDrawingToolTests
    {
        private record TestBoxDrawingTool : BoxDrawingTool
        {
            public PictureArea LastDrawnArea { get; private set; }

            protected override (Picture Picture, PictureArea Area) Draw(Drawer drawer, CoordinateHistory coordinateHistory, bool isShift)
            {
                var pic = Picture.CreateEmpty(new PictureSize(10, 10));
                // Just to mock return a predictable area based on coordinates
                var minX = Math.Min(coordinateHistory.Start.X, coordinateHistory.Now.X);
                var minY = Math.Min(coordinateHistory.Start.Y, coordinateHistory.Now.Y);
                var maxX = Math.Max(coordinateHistory.Start.X, coordinateHistory.Now.X);
                var maxY = Math.Max(coordinateHistory.Start.Y, coordinateHistory.Now.Y);

                var area = new PictureArea(new Position(minX, minY), new PictureSize(maxX - minX + 1, maxY - minY + 1));
                LastDrawnArea = area;
                return (pic, area);
            }

            public Position TestCalculateShiftedPosition(Position start, Position end)
            {
                return CalculateShiftedPosition(start, end);
            }
        }

        private DrawingBuffer CreateBuffer()
        {
            var pic = Picture.CreateEmpty(new PictureSize(32, 32));
            return new DrawingBuffer(pic);
        }

        private PenStyle CreatePenStyle()
        {
            return new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
        }

        [Test]
        public void DrawStart_Drawing_DrawEnd_CalculatesAffectedArea()
        {
            var tool = new TestBoxDrawingTool();
            var buffer = CreateBuffer();
            var penStyle = CreatePenStyle();

            var startCoord = new CanvasCoordinate(5, 5);
            var history = new CoordinateHistory(startCoord);

            // DrawStart
            var bufferAfterStart = tool.DrawStart(buffer, penStyle, history, false);
            var area1 = tool.LastDrawnArea;

            // Drawing to new pos
            var drawCoord = new CanvasCoordinate(8, 10);
            var drawHistory = history.Update(drawCoord);
            var bufferAfterDrawing = tool.Drawing(bufferAfterStart, penStyle, drawHistory, false);
            var area2 = tool.LastDrawnArea;

            // DrawEnd to final pos
            var endCoord = new CanvasCoordinate(15, 20);
            var endHistory = drawHistory.Update(endCoord);
            var result = tool.DrawEnd(bufferAfterDrawing, penStyle, endHistory, false);
            var area3 = tool.LastDrawnArea;

            var expectedCombinedArea = area2.Combine(area3);

            // Resulting area from DrawEnd should be the combination of affected areas
            Assert.That(result.AffectedArea.GetBoundingBox().Position.X, Is.EqualTo(expectedCombinedArea.Position.X));
            Assert.That(result.AffectedArea.GetBoundingBox().Position.Y, Is.EqualTo(expectedCombinedArea.Position.Y));
            Assert.That(result.AffectedArea.GetBoundingBox().Size.Width, Is.EqualTo(expectedCombinedArea.Size.Width));
            Assert.That(result.AffectedArea.GetBoundingBox().Size.Height, Is.EqualTo(expectedCombinedArea.Size.Height));
        }

        [Test]
        public void DrawEnd_WithoutPreviousDrawing_UsesResultAreaDirectly()
        {
            var tool = new TestBoxDrawingTool();
            var buffer = CreateBuffer();
            var penStyle = CreatePenStyle();

            var history = new CoordinateHistory(new CanvasCoordinate(5, 5));
            // DrawStart や Drawing を一度も呼ばずに直接 DrawEnd を呼ぶ (AffectedArea は null)
            var result = tool.DrawEnd(buffer, penStyle, history, false);

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
        public void DrawEnd_WhenShrunkAfterExpansion_CombinesAllAffectedAreas()
        {
            var tool = new TestBoxDrawingTool();
            var buffer = CreateBuffer();
            var penStyle = CreatePenStyle();

            var startCoord = new CanvasCoordinate(5, 5);
            var history = new CoordinateHistory(startCoord);

            // DrawStart at (5, 5) -> Area: (5, 5, 1, 1)
            var b1 = tool.DrawStart(buffer, penStyle, history, false);

            // Drawing to large area (20, 20) -> Area: (5, 5, 16, 16)
            var drawHistory = history.Update(new CanvasCoordinate(20, 20));
            var b2 = tool.Drawing(b1, penStyle, drawHistory, false);

            // DrawEnd shrunk to small area (6, 6) -> Area: (5, 5, 2, 2)
            var endHistory = drawHistory.Update(new CanvasCoordinate(6, 6));
            var result = tool.DrawEnd(b2, penStyle, endHistory, false);

            // Result must be Combined (5, 5, 16, 16) to clean up previous large area, NOT just the shrunk (5, 5, 2, 2)
            var bounds = result.AffectedArea.GetBoundingBox();
            Assert.Multiple(() =>
            {
                Assert.That(bounds.Position.X, Is.EqualTo(5));
                Assert.That(bounds.Position.Y, Is.EqualTo(5));
                Assert.That(bounds.Size.Width, Is.EqualTo(16), "Width should cover previous expanded drawing area");
                Assert.That(bounds.Size.Height, Is.EqualTo(16), "Height should cover previous expanded drawing area");
            });
        }

        [TestCase(10, 10, 15, 20, 20, 20)] // positive shift, height diff > width diff
        [TestCase(10, 10, 20, 15, 20, 20)] // positive shift, width diff > height diff
        [TestCase(10, 10, 5, 2, 2, 2)] // negative shift, height diff > width diff (8)
        [TestCase(10, 10, 2, 5, 2, 2)] // negative shift, width diff > height diff (8)
        [TestCase(10, 10, 15, 2, 18, 2)] // mixed: positive X, negative Y
        [TestCase(10, 10, 2, 15, 2, 18)] // mixed: negative X, positive Y
        [TestCase(10, 10, 10, 10, 10, 10)] // same point
        [TestCase(10, 10, 10, 15, 15, 15, Description = "垂直移動 (X差分0: end.X >= start.X 境界値)")]
        [TestCase(10, 10, 15, 10, 15, 15, Description = "水平移動 (Y差分0: end.Y >= start.Y 境界値)")]
        public void CalculateShiftedPosition_ReturnsExpectedPosition(int startX, int startY, int endX, int endY, int expectedX, int expectedY)
        {
            var tool = new TestBoxDrawingTool();
            var start = new Position(startX, startY);
            var end = new Position(endX, endY);

            var result = tool.TestCalculateShiftedPosition(start, end);

            Assert.That(result.X, Is.EqualTo(expectedX));
            Assert.That(result.Y, Is.EqualTo(expectedY));
        }

        [Test]
        public void DrawEnd_WithoutPriorDrawing_ReturnsCombinedWithStartArea()
        {
            // Arrange
            var tool = new TestBoxDrawingTool();
            var buffer = CreateBuffer();
            var penStyle = CreatePenStyle();

            var startCoord = new CanvasCoordinate(5, 5);
            var history = new CoordinateHistory(startCoord);

            // DrawStart のみ実行 (Drawing を挟まない)
            var bufferAfterStart = tool.DrawStart(buffer, penStyle, history, false);
            var startArea = tool.LastDrawnArea;

            // 直後に DrawEnd を実行
            var endCoord = new CanvasCoordinate(15, 20);
            var endHistory = history.Update(endCoord);
            var result = tool.DrawEnd(bufferAfterStart, penStyle, endHistory, false);
            var endArea = tool.LastDrawnArea;

            // Assert: Start 時のエリアと End 時のエリアが正しく結合される
            var expectedCombined = startArea.Combine(endArea);
            var bounds = result.AffectedArea.GetBoundingBox();
            Assert.Multiple(() =>
            {
                Assert.That(bounds.Position.X, Is.EqualTo(expectedCombined.Position.X));
                Assert.That(bounds.Position.Y, Is.EqualTo(expectedCombined.Position.Y));
                Assert.That(bounds.Size.Width, Is.EqualTo(expectedCombined.Size.Width));
                Assert.That(bounds.Size.Height, Is.EqualTo(expectedCombined.Size.Height));
            });
        }
    }
}
