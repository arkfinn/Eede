using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Filters;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.ImageEditing.Filters
{
    [TestFixture]
    public class AntiAliasFilterTests
    {
        [Test]
        public void Apply_OnDiamond_IsSymmetric()
        {
            // 7x7 White Image
            int w = 7;
            int h = 7;
            var data = new byte[w * h * 4];
            for (int i = 0; i < data.Length; i++) data[i] = 255;

            // Diamond pattern (Black pixels)
            //       (3,1)
            // (2,2) (3,2) (4,2)
            // (1,3) (2,3) (3,3) (4,3) (5,3)
            // (2,4) (3,4) (4,4)
            //       (3,5)
            SetPixel(data, 3, 1, w, 0, 0, 0, 255);
            SetPixel(data, 2, 2, w, 0, 0, 0, 255);
            SetPixel(data, 3, 2, w, 0, 0, 0, 255);
            SetPixel(data, 4, 2, w, 0, 0, 0, 255);
            SetPixel(data, 1, 3, w, 0, 0, 0, 255);
            SetPixel(data, 2, 3, w, 0, 0, 0, 255);
            SetPixel(data, 3, 3, w, 0, 0, 0, 255);
            SetPixel(data, 4, 3, w, 0, 0, 0, 255);
            SetPixel(data, 5, 3, w, 0, 0, 0, 255);
            SetPixel(data, 2, 4, w, 0, 0, 0, 255);
            SetPixel(data, 3, 4, w, 0, 0, 0, 255);
            SetPixel(data, 4, 4, w, 0, 0, 0, 255);
            SetPixel(data, 3, 5, w, 0, 0, 0, 255);

            var source = Picture.Create(new PictureSize(w, h), data);
            var filter = AntiAliasFilterFactory.Create(AntiAliasMode.Argb);
            var result = filter.Apply(source);

            // Check 4 diagonal neighbors (corners)
            // They should all have some blending (not 255 white, not 0 black)
            var pTL = result.PickColor(new Position(2, 2));
            var pTR = result.PickColor(new Position(4, 2));
            var pBL = result.PickColor(new Position(2, 4));
            var pBR = result.PickColor(new Position(4, 4));

            Assert.Multiple(() =>
            {
                Assert.That(pTL.Red, Is.GreaterThan(0).And.LessThan(255), "Top-Left corner should be blended");
                Assert.That(pTR.Red, Is.GreaterThan(0).And.LessThan(255), "Top-Right corner should be blended");
                Assert.That(pBL.Red, Is.GreaterThan(0).And.LessThan(255), "Bottom-Left corner should be blended");
                Assert.That(pBR.Red, Is.GreaterThan(0).And.LessThan(255), "Bottom-Right corner should be blended");
                
                // Symmetry check
                Assert.That(pTL.Red, Is.EqualTo(pTR.Red).Within(2), "Left/Right symmetry");
                Assert.That(pTL.Red, Is.EqualTo(pBL.Red).Within(2), "Top/Bottom symmetry");
            });
        }

        [Test]
        public void Apply_AlphaMode_DoesNotTouchRgb()
        {
            // Red image (255,0,0) with a transparent center
            int w = 3;
            int h = 3;
            var data = new byte[w * h * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = 0;       // B
                data[i + 1] = 0;   // G
                data[i + 2] = 255; // R
                data[i + 3] = 255; // A
            }
            // Set center to transparent but keep it Red (255,0,0,0)
            SetPixel(data, 1, 1, w, 255, 0, 0, 0);

            var source = Picture.Create(new PictureSize(w, h), data);
            var filter = AntiAliasFilterFactory.Create(AntiAliasMode.Alpha);
            var result = filter.Apply(source);

            // Check if any color changed
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var c = result.PickColor(new Position(x, y));
                    Assert.That(c.Red, Is.EqualTo(255), $"Red at ({x},{y}) should remain 255");
                    Assert.That(c.Green, Is.EqualTo(0), $"Green at ({x},{y}) should remain 0");
                    Assert.That(c.Blue, Is.EqualTo(0), $"Blue at ({x},{y}) should remain 0");
                }
            }
            // Center Alpha should have changed (increased from 0) due to AA
            var centerAlpha = result.PickColor(new Position(1, 1)).Alpha;
            Assert.That(centerAlpha, Is.GreaterThan(0), "Center Alpha should be blended");
        }

        [Test]
        public void Apply_RgbMode_DoesNotTouchAlpha()
        {
            // White image with a black center, all 128 alpha
            int w = 3;
            int h = 3;
            var data = new byte[w * h * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = 255;     // B
                data[i + 1] = 255; // G
                data[i + 2] = 255; // R
                data[i + 3] = 128; // A
            }
            SetPixel(data, 1, 1, w, 0, 0, 0, 128);

            var source = Picture.Create(new PictureSize(w, h), data);
            var filter = AntiAliasFilterFactory.Create(AntiAliasMode.Rgb);
            var result = filter.Apply(source);

            // Check if alpha changed
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var c = result.PickColor(new Position(x, y));
                    Assert.That(c.Alpha, Is.EqualTo(128), $"Alpha at ({x},{y}) should remain 128");
                }
            }
            // Center Red should have changed (increased from 0) due to AA
            var centerRed = result.PickColor(new Position(1, 1)).Red;
            Assert.That(centerRed, Is.GreaterThan(0), "Center RGB should be blended");
        }

        private bool IsIdentical(Picture a, Picture b)
        {
            if (a.Width != b.Width || a.Height != b.Height) return false;
            var spanA = a.AsSpan();
            var spanB = b.AsSpan();
            return spanA.SequenceEqual(spanB);
        }

        private void SetPixel(byte[] data, int x, int y, int strideWidth, byte r, byte g, byte b, byte a)
        {
            int idx = (y * strideWidth + x) * 4;
            data[idx] = b;
            data[idx + 1] = g;
            data[idx + 2] = r;
            data[idx + 3] = a;
        }
    }
}
