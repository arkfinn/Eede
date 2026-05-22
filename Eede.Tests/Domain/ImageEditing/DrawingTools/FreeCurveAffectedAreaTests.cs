using System.Linq;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools
{
    [TestFixture]
    public class FreeCurveAffectedAreaTests
    {
        [Test]
        public void AffectedArea_ShouldBeBoundingBoxOfAllSegments()
        {
            // Arrange
            var initialPicture = Picture.Create(new PictureSize(100, 100), new byte[100 * 100 * 4]);
            var buffer = new DrawingBuffer(initialPicture);
            var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
            var tool = new FreeCurve();

            // Act
            // Start at (10, 10)
            var pos1 = new CoordinateHistory(new CanvasCoordinate(10, 10));
            var b1 = tool.DrawStart(buffer, penStyle, pos1, false);

            // Move to (20, 20)
            var pos2 = pos1.Update(new CanvasCoordinate(20, 20));
            var b2 = tool.Drawing(b1, penStyle, pos2, false);

            // Move to (15, 30)
            var pos3 = pos2.Update(new CanvasCoordinate(15, 30));
            var b3 = tool.Drawing(b2, penStyle, pos3, false);

            var result = tool.DrawEnd(b3, penStyle, pos3, false);

            // Assert
            Assert.That(result.AffectedArea.IsEmpty, Is.False);
            var boundingBox = result.AffectedArea.GetBoundingBox();
            Assert.That(boundingBox.X, Is.EqualTo(10), "X mismatch");
            Assert.That(boundingBox.Y, Is.EqualTo(10), "Y mismatch");
            Assert.That(boundingBox.Width, Is.EqualTo(11), "Width mismatch");
            Assert.That(boundingBox.Height, Is.EqualTo(21), "Height mismatch");
        }

        [Test]
        public void AffectedArea_ShouldContainMultipleSegments()
        {
            // Arrange
            var initialPicture = Picture.Create(new PictureSize(100, 100), new byte[100 * 100 * 4]);
            var buffer = new DrawingBuffer(initialPicture);
            var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
            var tool = new FreeCurve();

            // Act
            var pos1 = new CoordinateHistory(new CanvasCoordinate(10, 10));
            tool.DrawStart(buffer, penStyle, pos1, false);
            var pos2 = pos1.Update(new CanvasCoordinate(20, 20));
            tool.Drawing(buffer, penStyle, pos2, false);
            var result = tool.DrawEnd(buffer, penStyle, pos2, false);

            // Assert
            // One for DrawStart (Point at 10,10), one for Drawing (Line 10,10 to 20,20)
            Assert.That(result.AffectedArea.Count(), Is.EqualTo(2));
        }
    }
}
