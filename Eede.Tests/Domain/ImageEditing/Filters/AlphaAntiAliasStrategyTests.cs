using Eede.Domain.ImageEditing.Filters;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.Filters
{
    [TestFixture]
    public class AlphaAntiAliasStrategyTests
    {
        private AlphaAntiAliasStrategy _strategy;

        [SetUp]
        public void Setup()
        {
            _strategy = new AlphaAntiAliasStrategy();
        }

        [Test]
        public unsafe void IsDifferent_ExceedsThreshold_ReturnsTrue()
        {
            byte[] p1 = new byte[] { 0, 0, 0, 0 }; // Alpha = 0
            byte[] p2 = new byte[] { 0, 0, 0, 255 }; // Alpha = 255

            fixed (byte* ptr1 = p1)
            fixed (byte* ptr2 = p2)
            {
                Assert.That(_strategy.IsDifferent(ptr1, ptr2, 0.5f), Is.True);
            }
        }

        [Test]
        public unsafe void IsDifferent_BelowThreshold_ReturnsFalse()
        {
            byte[] p1 = new byte[] { 0, 0, 0, 255 }; // Alpha = 255
            byte[] p2 = new byte[] { 0, 0, 0, 250 }; // Alpha = 250 (difference = 5/255 = 0.0196)

            fixed (byte* ptr1 = p1)
            fixed (byte* ptr2 = p2)
            {
                Assert.That(_strategy.IsDifferent(ptr1, ptr2, 0.5f), Is.False);
            }
        }

        [Test]
        public unsafe void GetValueForEdgeDetection_ReturnsAlpha()
        {
            byte[] p = new byte[] { 255, 255, 255, 128 }; // Alpha = 128

            fixed (byte* ptr = p)
            {
                float val = _strategy.GetValueForEdgeDetection(ptr);
                Assert.That(val, Is.EqualTo(128 / 255.0f).Within(0.001f));
            }
        }

        [Test]
        public unsafe void Blend_BlendsAlphaAndDoesNotTouchRgb()
        {
            byte[] src = new byte[] { 255, 255, 255, 255 }; // Alpha = 255
            byte[] dst = new byte[] { 10, 20, 30, 0 };      // Alpha = 0

            fixed (byte* srcPtr = src)
            fixed (byte* dstPtr = dst)
            {
                // a = 1.0f
                // w = clamp(a * 0.85f, 0, 1) -> w = 0.85f
                // d[3] = pT[3] * (1 - w) + pN[3] * w
                // d[3] = 0 * 0.15 + 255 * 0.85 = 216.75 -> 216
                _strategy.Blend(srcPtr, dstPtr, 0, 0, 0, 0, 4, 1.0f);
            }

            Assert.Multiple(() =>
            {
                Assert.That(dst[0], Is.EqualTo(10), "Blue channel should not be touched");
                Assert.That(dst[1], Is.EqualTo(20), "Green channel should not be touched");
                Assert.That(dst[2], Is.EqualTo(30), "Red channel should not be touched");
                Assert.That(dst[3], Is.EqualTo(216), "Alpha channel blended incorrectly");
            });
        }

        [Test]
        public unsafe void Blend_ExceedingWeight_ClampsCorrectly()
        {
            byte[] src = new byte[] { 0, 0, 0, 255 }; // Alpha = 255
            byte[] dst = new byte[] { 0, 0, 0, 0 };   // Alpha = 0

            fixed (byte* srcPtr = src)
            fixed (byte* dstPtr = dst)
            {
                // Weight is 2.0, but clamped: clamp(2.0 * 0.85, 0, 1) = clamp(1.7, 0, 1) = 1.0
                // So w = 1.0. d[3] = pN[3] * 1.0 = 255
                _strategy.Blend(srcPtr, dstPtr, 0, 0, 0, 0, 4, 2.0f);
            }

            Assert.Multiple(() =>
            {
                Assert.That(dst[3], Is.EqualTo(255));
            });
        }

        [Test]
        public unsafe void Blend_ZeroWeight_DoesNotChangeDst()
        {
            byte[] src = new byte[] { 0, 0, 0, 255 }; // Alpha = 255
            byte[] dst = new byte[] { 0, 0, 0, 100 }; // Alpha = 100

            fixed (byte* srcPtr = src)
            fixed (byte* dstPtr = dst)
            {
                // w = clamp(0 * 0.85, 0, 1) = 0.0
                _strategy.Blend(srcPtr, dstPtr, 0, 0, 0, 0, 4, 0.0f);
            }

            Assert.Multiple(() =>
            {
                Assert.That(dst[3], Is.EqualTo(100));
            });
        }
    }
}
