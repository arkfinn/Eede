using Eede.Domain.Scales;
using Eede.Domain.SharedKernel;
using Eede.Domain.Sizes;
using NUnit.Framework;

namespace Eede.Domain.Tests.Sizes
{
    [TestFixture]
    public class MagnifiedSizeTests
    {
        [Test]
        public void MagnifiedSizeTest()
        {
            MagnifiedSize size = new(new PictureSize(8, 4), new Magnification(8));
            Assert.That(new int[] { size.Width, size.Height }, Is.EqualTo(new int[] { 64, 32 }));
        }
    }
}