using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.SharedKernel
{
    [TestFixture]
    public class CoordinateTests
    {
        [TestCase(100, 200, 2.0f, 50, 100)]
        [TestCase(5, 5, 2.0f, 2, 2)]
        [TestCase(-5, -5, 2.0f, -3, -3)] // Note: -5 / 2.0f = -2.5. Math.Floor(-2.5) = -3.
        [TestCase(100, 200, 0.5f, 200, 400)]
        [TestCase(0, 0, 2.0f, 0, 0)]
        [TestCase(100, 200, 1.0f, 100, 200)]
        public void ToCanvasTest(int displayX, int displayY, float magValue, int expectedCanvasX, int expectedCanvasY)
        {
            var display = new DisplayCoordinate(displayX, displayY);
            var mag = new Magnification(magValue);
            var canvas = display.ToCanvas(mag);

            Assert.Multiple(() =>
            {
                Assert.That(canvas.X, Is.EqualTo(expectedCanvasX));
                Assert.That(canvas.Y, Is.EqualTo(expectedCanvasY));
            });
        }

        [TestCase(50, 100, 2.0f, 100, 200)]
        [TestCase(2, 2, 2.0f, 4, 4)]
        [TestCase(-3, -3, 2.0f, -6, -6)]
        [TestCase(200, 400, 0.5f, 100, 200)]
        [TestCase(0, 0, 2.0f, 0, 0)]
        [TestCase(100, 200, 1.0f, 100, 200)]
        public void ToDisplayTest(int canvasX, int canvasY, float magValue, int expectedDisplayX, int expectedDisplayY)
        {
            var canvas = new CanvasCoordinate(canvasX, canvasY);
            var mag = new Magnification(magValue);
            var display = canvas.ToDisplay(mag);

            Assert.Multiple(() =>
            {
                Assert.That(display.X, Is.EqualTo(expectedDisplayX));
                Assert.That(display.Y, Is.EqualTo(expectedDisplayY));
            });
        }
    }
}
