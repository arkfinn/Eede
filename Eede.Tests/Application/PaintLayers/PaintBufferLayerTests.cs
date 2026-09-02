using Eede.Application.PaintLayers;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Application.Tests.PaintLayers
{
    [TestFixture]
    public class PaintBufferLayerTests
    {
        private class DirectImageTransfer : IImageTransfer
        {
            public Picture Transfer(Picture source, Magnification magnification)
            {
                return source;
            }
        }

        [Test]
        public void Painted_WithNullSource_ReturnsDestination()
        {
            var paintSize = new MagnifiedSize(new PictureSize(10, 10), new Magnification(1.0f));
            var transfer = new DirectImageTransfer();
            var layer = new PaintBufferLayer(paintSize, null!, transfer);

            var destination = Picture.CreateEmpty(new PictureSize(10, 10));

            var result = layer.Painted(destination);

            Assert.That(result, Is.SameAs(destination));
        }

        [Test]
        public void Painted_WithValidSource_TransfersSource()
        {
            var paintSize = new MagnifiedSize(new PictureSize(10, 10), new Magnification(1.0f));
            var transfer = new DirectImageTransfer();

            var source = Picture.CreateEmpty(new PictureSize(5, 5));
            var destination = Picture.CreateEmpty(new PictureSize(10, 10));

            var layer = new PaintBufferLayer(paintSize, source, transfer);

            var result = layer.Painted(destination);

            // Note: PaintBufferLayer ignores destination when Source is not null.
            Assert.That(result, Is.SameAs(source));
        }
    }
}
