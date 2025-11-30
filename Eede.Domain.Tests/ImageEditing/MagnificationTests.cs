using Eede.Domain.ImageEditing;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.ImageEditing
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
            Assert.That(m.Magnify(3), Is.EqualTo(12));
        }

        [Test]
        public void EqualsTest()
        {
            Assert.That(new Magnification(4), Is.EqualTo(new Magnification(4)));
        }

        [Test]
        public void GetHashCodeTest()
        {
            Assert.That(new Magnification(4).GetHashCode(), Is.EqualTo(new Magnification(4).GetHashCode()));
        }
    }
}