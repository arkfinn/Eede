using NUnit.Framework;
using System;

namespace Eede.Tests
{
    [TestFixture]
    public class MagnificationTests
    {
        [Test]
        public void OutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Magnification(0);
            });
        }

        [Test]
        public void MagnifyTest()
        {
            var m = new Magnification(4);
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