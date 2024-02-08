using Eede.Domain.Scales;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.Scales
{
    [TestFixture]
    public class MagnificationTests
    {
        [Test]
        public void OutOfRange()
        {
            _ = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new Magnification(0);
            });
        }

        [Test]
        public void MagnifyTest()
        {
            Magnification m = new(4);
            Assert.AreEqual(12, m.Magnify(3));
        }

        [Test]
        public void EqualsTest()
        {
            Assert.AreEqual(new Magnification(4), new Magnification(4));
        }

        [Test]
        public void GetHashCodeTest()
        {
            Assert.AreEqual(new Magnification(4).GetHashCode(), new Magnification(4).GetHashCode());
        }
    }
}