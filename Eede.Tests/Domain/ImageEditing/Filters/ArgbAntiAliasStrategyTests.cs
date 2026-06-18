using Eede.Domain.ImageEditing.Filters;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.Filters
{
    [TestFixture]
    public class ArgbAntiAliasStrategyTests
    {
        private ArgbAntiAliasStrategy _strategy;

        [SetUp]
        public void Setup()
        {
            _strategy = new ArgbAntiAliasStrategy();
        }

        [Test]
        public unsafe void IsDifferent_ExceedsThreshold_Luminance_ReturnsTrue()
        {
            byte[] p1 = new byte[] { 0, 0, 0, 255 }; // Black
            byte[] p2 = new byte[] { 255, 255, 255, 255 }; // White

            fixed (byte* ptr1 = p1)
            fixed (byte* ptr2 = p2)
            {
                Assert.That(_strategy.IsDifferent(ptr1, ptr2, 0.5f), Is.True);
            }
        }

        [Test]
        public unsafe void IsDifferent_ExceedsThreshold_Alpha_ReturnsTrue()
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
            byte[] p1 = new byte[] { 0, 0, 0, 255 }; // Black
            byte[] p2 = new byte[] { 10, 10, 10, 250 }; // Dark Gray, slightly transparent

            fixed (byte* ptr1 = p1)
            fixed (byte* ptr2 = p2)
            {
                Assert.That(_strategy.IsDifferent(ptr1, ptr2, 0.5f), Is.False);
            }
        }

        [Test]
        public unsafe void GetValueForEdgeDetection_ReturnsLuminance()
        {
            // Luminance = (0.299f * R + 0.587f * G + 0.114f * B) / 255.0f
            // BGRA layout
            // Red: B=0, G=0, R=255
            byte[] p = new byte[] { 0, 0, 255, 255 };

            fixed (byte* ptr = p)
            {
                float val = _strategy.GetValueForEdgeDetection(ptr);
                Assert.That(val, Is.EqualTo(0.299f).Within(0.001f));
            }
        }

        [Test]
        public unsafe void Blend_BlendsAllChannels()
        {
            byte[] src = new byte[] { 255, 255, 255, 255 }; // White
            byte[] dst = new byte[] { 0, 0, 0, 0 }; // Transparent Black

            fixed (byte* srcPtr = src)
            fixed (byte* dstPtr = dst)
            {
                // w = clamp(a * 0.85f, 0, 1) -> a = 1 -> w = 0.85
                // d[i] = (byte)(pT[i] * (1 - w) + pN[i] * w)
                // d[i] = (byte)(0 * 0.15 + 255 * 0.85) = (byte)(216.75) = 216
                _strategy.Blend(srcPtr, dstPtr, 0, 0, 0, 0, 4, 1.0f);
            }

            Assert.Multiple(() =>
            {
                Assert.That(dst[0], Is.EqualTo(216), "Blue channel blended incorrectly");
                Assert.That(dst[1], Is.EqualTo(216), "Green channel blended incorrectly");
                Assert.That(dst[2], Is.EqualTo(216), "Red channel blended incorrectly");
                Assert.That(dst[3], Is.EqualTo(216), "Alpha channel blended incorrectly");
            });
        }

        [Test]
        public unsafe void Blend_ExceedingWeight_ClampsCorrectly()
        {
            byte[] src = new byte[] { 255, 255, 255, 255 }; // White
            byte[] dst = new byte[] { 0, 0, 0, 0 }; // Transparent Black

            fixed (byte* srcPtr = src)
            fixed (byte* dstPtr = dst)
            {
                // Weight is 2.0, but clamped: clamp(2.0 * 0.85, 0, 1) = clamp(1.7, 0, 1) = 1.0
                // So w = 1.0. d[i] = pN[i] * 1.0 = 255
                _strategy.Blend(srcPtr, dstPtr, 0, 0, 0, 0, 4, 2.0f);
            }

            Assert.Multiple(() =>
            {
                Assert.That(dst[0], Is.EqualTo(255));
                Assert.That(dst[1], Is.EqualTo(255));
                Assert.That(dst[2], Is.EqualTo(255));
                Assert.That(dst[3], Is.EqualTo(255));
            });
        }

        [Test]
        public unsafe void Blend_ZeroWeight_DoesNotChangeDst()
        {
            byte[] src = new byte[] { 255, 255, 255, 255 }; // White
            byte[] dst = new byte[] { 100, 100, 100, 100 }; // Gray

            fixed (byte* srcPtr = src)
            fixed (byte* dstPtr = dst)
            {
                // w = clamp(0 * 0.85, 0, 1) = 0.0
                _strategy.Blend(srcPtr, dstPtr, 0, 0, 0, 0, 4, 0.0f);
            }

            Assert.Multiple(() =>
            {
                Assert.That(dst[0], Is.EqualTo(100));
                Assert.That(dst[1], Is.EqualTo(100));
                Assert.That(dst[2], Is.EqualTo(100));
                Assert.That(dst[3], Is.EqualTo(100));
            });
        }
    }
}
