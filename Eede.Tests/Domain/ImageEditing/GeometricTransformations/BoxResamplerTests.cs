using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.ImageEditing.GeometricTransformations
{
    [TestFixture]
    public class BoxResamplerTests
    {
        [TestCase(0, 10)]
        [TestCase(10, 0)]
        [TestCase(0, 0)]
        public void Resize_ThrowsArgumentException_WhenSizeIsInvalid(int width, int height)
        {
            var data = new byte[4];
            var source = Picture.Create(new PictureSize(1, 1), data);
            var resampler = new BoxResampler();
            Assert.Catch<ArgumentException>(() => resampler.Resize(source, new PictureSize(width, height)));
        }

        [Test]
        public void Resize_2x2_to_1x1_AveragesPixels()
        {
            // 2x2 Image
            // [R:10, G:20, B:30, A:40], [R:20, G:30, B:40, A:50]
            // [R:30, G:40, B:50, A:60], [R:40, G:50, B:60, A:70]
            // Average should be: [R:25, G:35, B:45, A:55]
            int w = 2;
            int h = 2;
            var data = new byte[w * h * 4];

            SetPixel(data, 0, 0, w, 10, 20, 30, 40);
            SetPixel(data, 1, 0, w, 20, 30, 40, 50);
            SetPixel(data, 0, 1, w, 30, 40, 50, 60);
            SetPixel(data, 1, 1, w, 40, 50, 60, 70);

            var source = Picture.Create(new PictureSize(w, h), data);
            var resampler = new BoxResampler();
            var newSize = new PictureSize(1, 1);

            var result = resampler.Resize(source, newSize);

            Assert.That(result.Width, Is.EqualTo(1));
            Assert.That(result.Height, Is.EqualTo(1));

            AssertColor(result, 0, 0, 25, 35, 45, 55);
        }

        [Test]
        public void Resize_2x2_to_2x1_AveragesVertically()
        {
            // 2x2 Image
            // [R:10, G:20, B:30, A:40], [R:100, G:110, B:120, A:130]
            // [R:30, G:40, B:50, A:60], [R:120, G:130, B:140, A:150]
            // Average Left: [R:20, G:30, B:40, A:50]
            // Average Right: [R:110, G:120, B:130, A:140]
            int w = 2;
            int h = 2;
            var data = new byte[w * h * 4];

            SetPixel(data, 0, 0, w, 10, 20, 30, 40);
            SetPixel(data, 1, 0, w, 100, 110, 120, 130);
            SetPixel(data, 0, 1, w, 30, 40, 50, 60);
            SetPixel(data, 1, 1, w, 120, 130, 140, 150);

            var source = Picture.Create(new PictureSize(w, h), data);
            var resampler = new BoxResampler();
            var newSize = new PictureSize(2, 1);

            var result = resampler.Resize(source, newSize);

            Assert.That(result.Width, Is.EqualTo(2));
            Assert.That(result.Height, Is.EqualTo(1));

            AssertColor(result, 0, 0, 20, 30, 40, 50);
            AssertColor(result, 1, 0, 110, 120, 130, 140);
        }

        [Test]
        public void Resize_1x1_to_2x2_DuplicatesPixels()
        {
            // 1x1 Image
            // [R:100, G:150, B:200, A:250]
            int w = 1;
            int h = 1;
            var data = new byte[w * h * 4];

            SetPixel(data, 0, 0, w, 100, 150, 200, 250);

            var source = Picture.Create(new PictureSize(w, h), data);
            var resampler = new BoxResampler();
            var newSize = new PictureSize(2, 2);

            var result = resampler.Resize(source, newSize);

            Assert.That(result.Width, Is.EqualTo(2));
            Assert.That(result.Height, Is.EqualTo(2));

            AssertColor(result, 0, 0, 100, 150, 200, 250);
            AssertColor(result, 1, 0, 100, 150, 200, 250);
            AssertColor(result, 0, 1, 100, 150, 200, 250);
            AssertColor(result, 1, 1, 100, 150, 200, 250);
        }

        private void SetPixel(byte[] data, int x, int y, int strideWidth, byte r, byte g, byte b, byte a)
        {
            int idx = (y * strideWidth + x) * 4;
            data[idx] = b;
            data[idx + 1] = g;
            data[idx + 2] = r;
            data[idx + 3] = a;
        }

        private void AssertColor(Picture pic, int x, int y, byte r, byte g, byte b, byte a)
        {
            var color = pic.PickColor(new Position(x, y));
            Assert.Multiple(() =>
            {
                Assert.That(color.Red, Is.EqualTo(r), $"Red at {x},{y}");
                Assert.That(color.Green, Is.EqualTo(g), $"Green at {x},{y}");
                Assert.That(color.Blue, Is.EqualTo(b), $"Blue at {x},{y}");
                Assert.That(color.Alpha, Is.EqualTo(a), $"Alpha at {x},{y}");
            });
        }
    }
}
