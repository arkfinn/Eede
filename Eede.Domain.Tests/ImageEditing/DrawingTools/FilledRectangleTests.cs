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
            DrawingBuffer endBuffer = tool.DrawEnd(startBuffer, penStyle, endPos, false);

            // Assert
            Picture expected = PictureHelper.ReadBitmap(@"ImageEditing\DrawingTools\test\fillRectangle1.png");
            Assert.That(endBuffer.Fetch().CloneImage(), Is.EqualTo(expected.CloneImage()));
        }
    }
}
