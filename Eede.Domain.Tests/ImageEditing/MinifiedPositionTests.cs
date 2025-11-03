using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing
{
    [TestFixture]
    public class MinifiedPositionTests
    {
        [Test]
        public void MinifiedPositionTest()
        {
            MinifiedPosition pos = new(new Position(64, 32), new Magnification(8));
            Assert.That(pos.X, Is.EqualTo(8));
            Assert.That(pos.Y, Is.EqualTo(4));
            Assert.That(pos.ToPosition(), Is.EqualTo(new Position(8, 4)));
        }
    }
}