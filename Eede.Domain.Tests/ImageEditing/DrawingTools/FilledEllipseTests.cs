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
    public class FilledEllipseTests
    {
        [Test]
        public void DrawTest()
        {
            // Arrange
            Picture src = PictureHelper.ReadBitmap(@"ImageEditing\DrawingTools\test\base.png");
            DrawingBuffer buffer = new(src);
            PenStyle penStyle = new(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1);
            FilledEllipse tool = new();
            
            // DrawerTests uses (5, 5) to (9, 9) for fillEllipse1.png
            CoordinateHistory pos = new(new CanvasCoordinate(5, 5));
            
            // Act
            DrawingBuffer startBuffer = tool.DrawStart(buffer, penStyle, pos, false);
            CoordinateHistory endPos = pos.Update(new CanvasCoordinate(9, 9));
            var result = tool.DrawEnd(startBuffer, penStyle, endPos, false);

            // Assert
            Picture expected = PictureHelper.ReadBitmap(@"ImageEditing\DrawingTools\test\fillEllipse1.png");
            Assert.That(result.Buffer.Fetch().CloneImage(), Is.EqualTo(expected.CloneImage()));
        }
    }
}