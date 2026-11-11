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
    public class FilledRectangleTests
    {
        [Test]
        public void DrawTest()
        {
            // Arrange
            Picture src = PictureHelper.ReadBitmap(@"ImageEditing\DrawingTools\test\base.png");
            DrawingBuffer buffer = new(src);
            PenStyle penStyle = new(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1);
            FilledRectangle tool = new();

            // Start at (10, 10)
            CoordinateHistory pos = new(new CanvasCoordinate(10, 10));

            // Act
            DrawingBuffer startBuffer = tool.DrawStart(buffer, penStyle, pos, false);
            // Move to (15, 15)
            CoordinateHistory endPos = pos.Update(new CanvasCoordinate(15, 15));
            var result = tool.DrawEnd(startBuffer, penStyle, endPos, false);

            // Assert
            Picture expected = PictureHelper.ReadBitmap(@"ImageEditing\DrawingTools\test\fillRectangle1.png");
            Assert.That(result.Buffer.Fetch().CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [Test]
        public void DrawTest_Shift_SnapsToSquare()
        {
            // Arrange
            Picture src = PictureHelper.ReadBitmap(@"ImageEditing\DrawingTools\test\base.png");
            DrawingBuffer buffer = new(src);
            PenStyle penStyle = new(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1);
            FilledRectangle tool = new();

            // Start at (10, 10)
            CoordinateHistory pos = new(new CanvasCoordinate(10, 10));

            // Act: 非対称な (20, 15) へドラッグ
            DrawingBuffer startBuffer = tool.DrawStart(buffer, penStyle, pos, true);
            CoordinateHistory endPos = pos.Update(new CanvasCoordinate(20, 15));
            var result = tool.DrawEnd(startBuffer, penStyle, endPos, true);

            // Assert: isShift=true のため幅と高さが最大値 11 の正方形にスナップする
            var bounds = result.AffectedArea.GetBoundingBox();
            Assert.Multiple(() =>
            {
                Assert.That(bounds.Position.X, Is.EqualTo(10));
                Assert.That(bounds.Position.Y, Is.EqualTo(10));
                Assert.That(bounds.Size.Width, Is.EqualTo(11));
                Assert.That(bounds.Size.Height, Is.EqualTo(11));
            });
        }

        [Test]
        public void DrawTest_WithoutShift_DrawsNonSquareRectangle()
        {
            // Arrange
            Picture src = PictureHelper.ReadBitmap(@"ImageEditing\DrawingTools\test\base.png");
            DrawingBuffer buffer = new(src);
            PenStyle penStyle = new(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1);
            FilledRectangle tool = new();

            // Start at (10, 10)
            CoordinateHistory pos = new(new CanvasCoordinate(10, 10));

            // Act: 非対称な (20, 15) へドラッグ (isShift: false)
            DrawingBuffer startBuffer = tool.DrawStart(buffer, penStyle, pos, false);
            CoordinateHistory endPos = pos.Update(new CanvasCoordinate(20, 15));
            var result = tool.DrawEnd(startBuffer, penStyle, endPos, false);

            // Assert: isShift=false のため長方形（幅11, 高さ6）のまま維持される
            var bounds = result.AffectedArea.GetBoundingBox();
            Assert.Multiple(() =>
            {
                Assert.That(bounds.Size.Width, Is.EqualTo(11));
                Assert.That(bounds.Size.Height, Is.EqualTo(6));
            });
        }
    }
}
