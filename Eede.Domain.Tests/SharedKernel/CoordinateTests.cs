using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.SharedKernel
{
    [TestFixture]
    public class CoordinateTests
    {
        [Test]
        public void ToCanvasTest()
        {
            var display = new DisplayCoordinate(100, 200);
            var mag = new Magnification(2.0f);
            var canvas = display.ToCanvas(mag);

            Assert.Multiple(() =>
            {
                Assert.That(canvas.X, Is.EqualTo(50));
                Assert.That(canvas.Y, Is.EqualTo(100));
            });
        }
    }
}
