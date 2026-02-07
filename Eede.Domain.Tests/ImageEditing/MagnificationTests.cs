using Eede.Domain.ImageEditing;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.ImageEditing
{
    public class MagnificationTests
    {
        [Test]
        [TestCase(1f, 10, 10)]
        [TestCase(2f, 10, 20)]
        [TestCase(4f, 10, 40)]
        [TestCase(0.5f, 10, 5)]
        public void Magnify_CalculatesCorrectValue(float mag, int input, int expected)
        {
            var magnification = new Magnification(mag);
            Assert.That(magnification.Magnify(input), Is.EqualTo(expected));
        }

        [Test]
        [TestCase(1f, 10, 10)]
        [TestCase(2f, 20, 10)]
        [TestCase(4f, 40, 10)]
        [TestCase(2f, 21, 10)] // 切り捨て確認
        public void Minify_CalculatesCorrectValue(float mag, int input, int expected)
        {
            var magnification = new Magnification(mag);
            Assert.That(magnification.Minify(input), Is.EqualTo(expected));
        }

        [Test]
        public void Constructor_ThrowsException_ForZeroOrNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Magnification(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Magnification(-1));
        }
    }
}