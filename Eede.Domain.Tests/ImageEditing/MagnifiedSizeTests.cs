using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing
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