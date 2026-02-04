using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.ImageEditing
{
    [TestFixture]
    public class NearestNeighborResamplerTests
    {
        [Test]
        public void Resize_2x2_to_4x4_ExpandsPixels()
        {
            // 2x2 Image
            // [Red, Blue]
            // [Green, Yellow]
            int w = 2;
            int h = 2;
            var data = new byte[w * h * 4];
            
            // (0,0) Red
            SetPixel(data, 0, 0, w, 255, 0, 0, 255);
            // (1,0) Blue
            SetPixel(data, 1, 0, w, 0, 0, 255, 255);
            // (0,1) Green
            SetPixel(data, 0, 1, w, 0, 255, 0, 255);
            // (1,1) Yellow
            SetPixel(data, 1, 1, w, 255, 255, 0, 255);

            var source = Picture.Create(new PictureSize(w, h), data);
            var resampler = new NearestNeighborResampler();
            var newSize = new PictureSize(4, 4);

            var result = resampler.Resize(source, newSize);

            Assert.That(result.Width, Is.EqualTo(4));
            Assert.That(result.Height, Is.EqualTo(4));

            // Check (0,0), (0,1), (1,0), (1,1) should all be Red
            AssertColor(result, 0, 0, 255, 0, 0, 255);
            AssertColor(result, 1, 1, 255, 0, 0, 255);
            
            // Check (2,0) should be Blue
            AssertColor(result, 2, 0, 0, 0, 255, 255);
            
            // Check (0,2) should be Green
            AssertColor(result, 0, 2, 0, 255, 0, 255);

            // Check (2,2) should be Yellow
            AssertColor(result, 2, 2, 255, 255, 0, 255);
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
