using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing
{
    [TestFixture]
    public class IImageResamplerTests
    {
        private class DummyResampler : IImageResampler
        {
            public Picture Resize(Picture source, PictureSize newSize)
            {
                return Picture.Create(newSize, new byte[newSize.Width * newSize.Height * 4]);
            }
        }

        [Test]
        public void IImageResampler_CanBeImplemented()
        {
            IImageResampler resampler = new DummyResampler();
            var source = Picture.Create(new PictureSize(1, 1), new byte[4]);
            var newSize = new PictureSize(2, 2);

            var result = resampler.Resize(source, newSize);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Width, Is.EqualTo(2));
            Assert.That(result.Height, Is.EqualTo(2));
        }
    }
}
