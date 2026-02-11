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
    public class RectangleTests
    {
        [Test]
        public void DrawTest()
        {
            // Arrange
            Picture src = PictureHelper.ReadBitmap(@"ImageEditing\DrawingTools\test\base.png");
            DrawingBuffer buffer = new(src);
            // DrawerTests uses Red color
            PenStyle penStyle = new(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1);
            Rectangle tool = new();

            // Start at (10, 10)
            CoordinateHistory pos = new(new CanvasCoordinate(10, 10));

            // Act
            DrawingBuffer startBuffer = tool.DrawStart(buffer, penStyle, pos, false);
            // Move to (15, 15)
            CoordinateHistory endPos = pos.Update(new CanvasCoordinate(15, 15));
            var result = tool.DrawEnd(startBuffer, penStyle, endPos, false);

            // Assert
            Picture expected = PictureHelper.ReadBitmap(@"ImageEditing\DrawingTools\test\rectangle1.png");
            Assert.That(result.Buffer.Fetch().CloneImage(), Is.EqualTo(expected.CloneImage()));
        }
    }
}