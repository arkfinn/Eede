using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.SharedKernel
{
    [TestFixture]
    public class LineCoordinateTests
    {
        [Test]
        public void Constructor_SetsProperties()
        {
            var start = new Position(1, 2);
            var end = new Position(3, 4);
            var line = new LineCoordinate(start, end);

            Assert.That(line.Start, Is.EqualTo(start));
            Assert.That(line.End, Is.EqualTo(end));
        }

        [Test]
        public void Equality_SameStartAndEnd_AreEqual()
        {
            var start = new Position(1, 2);
            var end = new Position(3, 4);
            var line1 = new LineCoordinate(start, end);
            var line2 = new LineCoordinate(start, end);

            Assert.That(line1, Is.EqualTo(line2));
            Assert.That(line1.Equals(line2), Is.True);
            Assert.That(line1 == line2, Is.True);
            Assert.That(line1.GetHashCode(), Is.EqualTo(line2.GetHashCode()));
        }

        [Test]
        public void Inequality_DifferentStartOrEnd_AreNotEqual()
        {
            var start1 = new Position(1, 2);
            var end1 = new Position(3, 4);
            var start2 = new Position(5, 6);
            var end2 = new Position(7, 8);

            var line1 = new LineCoordinate(start1, end1);
            var line2 = new LineCoordinate(start2, end1);
            var line3 = new LineCoordinate(start1, end2);

            Assert.That(line1, Is.Not.EqualTo(line2));
            Assert.That(line1 != line2, Is.True);

            Assert.That(line1, Is.Not.EqualTo(line3));
            Assert.That(line1 != line3, Is.True);
        }
    }
}
