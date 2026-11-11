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
        public void EqualsTest()
        {
            var p1 = new Position(1, 2);
            var p2 = new Position(1, 2);
            Assert.That(p1, Is.EqualTo(p2));
        }

        [Test]
        public void OperatorPlusTest()
        {
            var p1 = new Position(10, 20);
            var p2 = new Position(5, 7);
            var result = p1 + p2;

            Assert.Multiple(() =>
            {
                Assert.That(result.X, Is.EqualTo(15));
                Assert.That(result.Y, Is.EqualTo(27));
            });
        }

        [Test]
        public void GetHashCodeTest()
        {
            var p1 = new Position(1, 2);
            var p2 = new Position(1, 2);
            Assert.That(p1.GetHashCode(), Is.EqualTo(p2.GetHashCode()));
        }
    }
}