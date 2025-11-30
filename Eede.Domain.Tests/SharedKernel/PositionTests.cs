using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.SharedKernel
{
    [TestFixture]
    public class PositionTests
    {
        [Test]
        public void PositionTest()
        {
            Position p = new(1, 2);
            Assert.That(p.X, Is.EqualTo(1));
            Assert.That(p.Y, Is.EqualTo(2));
        }

        [Test]
        public void PositionCreateFromPoint()
        {
            Position p = new(1, 2);
            Assert.That(p.X, Is.EqualTo(1));
            Assert.That(p.Y, Is.EqualTo(2));
        }

        [Test]
        public void GetHashCodeTest()
        {
            Assert.That(new Position(1, 2), Is.EqualTo(new Position(1, 2)));
        }

        [Test]
        public void ToPointTest()
        {
            Assert.That(new Position(1, 2).GetHashCode(), Is.EqualTo(new Position(1, 2).GetHashCode()));
        }
    }
}