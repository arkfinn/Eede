using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.ImageEditing
{
    [TestFixture]
    public class PictureTests
    {
        [TestCaseSource(nameof(PickColorCases))]
        public void PickColorTest(int x, int y, ArgbColor expected)
        {
            var darkSeaGreen = new ArgbColor(255, 143, 188, 143);
            var beige = new ArgbColor(255, 245, 245, 220);

            Picture source = CreateSamplePicture(10, 10,
                (1, 2, darkSeaGreen),
                (2, 3, beige)
            );
            ArgbColor color = source.PickColor(new Position(x, y));
            Assert.That(color, Is.EqualTo(expected));
        }

        private static readonly object[] PickColorCases =
        {
            new object[] { 1, 2, new ArgbColor(255, 143, 188, 143) },
            new object[] { 2, 3, new ArgbColor(255, 245, 245, 220) },
        };

        [TestCaseSource(nameof(CreateEmptyCases))]
        public void CreateEmptyTest(int width, int height, int expectedLength)
        {
            var size = new PictureSize(width, height);
            Picture picture = Picture.CreateEmpty(size);

            Assert.That(picture.Size, Is.EqualTo(size));
            Assert.That(picture.Width, Is.EqualTo(width));
            Assert.That(picture.Height, Is.EqualTo(height));
            Assert.That(picture.Length, Is.EqualTo(expectedLength));

            // Check if all elements are zero
            byte[] imageBytes = picture.CloneImage();
            Assert.That(imageBytes, Is.All.Zero);
        }

        private static readonly object[] CreateEmptyCases =
        {
            new object[] { 10, 10, 400 },
            new object[] { 1, 1, 4 },
            new object[] { 0, 0, 0 },
        };

        [Test]
        public void PickColorの引数posはbitmapの範囲外を許容しない()
        {
            _ = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Picture source = Picture.Create(new PictureSize(10, 10), new byte[10 * 4 * 10]);
                _ = source.PickColor(new Position(10, 10));
            });
        }

        [TestCaseSource(nameof(CutOutCases))]
        public void CutOutTest(int x, int y, ArgbColor expected)
        {
            var darkSeaGreen = new ArgbColor(255, 143, 188, 143);
            var beige = new ArgbColor(255, 245, 245, 220);

            Picture source = CreateSamplePicture(10, 10,
                (1, 2, darkSeaGreen),
                (2, 3, beige)
            );

            Picture d = source.CutOut(new PictureArea(new Position(1, 2), new PictureSize(5, 6)));

            ArgbColor color = d.PickColor(new Position(x, y));
            Assert.That(color, Is.EqualTo(expected));
        }

        private static readonly object[] CutOutCases =
        {
            new object[] { 0, 0, new ArgbColor(255, 143, 188, 143) },
            new object[] { 1, 1, new ArgbColor(255, 245, 245, 220) },
        };

        [Test]
        public void CutOutErrorTest()
        {
            // 画像自体のサイズより大きいカットサイズを指定する場合、余りは空白で埋める
            Picture picture = Picture.Create(new PictureSize(2, 2), new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8 });
            Picture after = picture.CutOut(new PictureArea(new Position(0, 0), new PictureSize(4, 4)));
            Assert.That(after.CloneImage(), Is.EqualTo(new byte[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 0, 0, 0, 0, 0, 0, 0, 0,
                1, 2, 3, 4, 5, 6, 7, 8, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            }));
        }

        [Test]
        public void CutOut_WithNegativeCoordinates_FillsOutsideWithTransparentAndCopiesInside()
        {
            // 2x2 画像に対して (-1, -1) から 3x3 で切り出す
            // 画像は (0,0)〜(1,1) に存在し、切り出し結果の (1,1)〜(2,2) にマッピングされる
            Picture picture = Picture.Create(new PictureSize(2, 2), new byte[] {
                10, 20, 30, 40,  50, 60, 70, 80,
                11, 21, 31, 41,  51, 61, 71, 81
            });
            Picture after = picture.CutOut(new PictureArea(new Position(-1, -1), new PictureSize(3, 3)));

            Assert.That(after.Size, Is.EqualTo(new PictureSize(3, 3)));
            byte[] bytes = after.CloneImage();
            // row 0 (y=-1): 全部 0
            // row 1 (y=0): x=-1(0), x=0(10..40), x=1(50..80)
            // row 2 (y=1): x=-1(0), x=0(11..41), x=1(51..81)
            byte[] expected = new byte[]
            {
                0, 0, 0, 0,    0, 0, 0, 0,     0, 0, 0, 0,
                0, 0, 0, 0,   10, 20, 30, 40, 50, 60, 70, 80,
                0, 0, 0, 0,   11, 21, 31, 41, 51, 61, 71, 81,
            };
            Assert.That(bytes, Is.EqualTo(expected));
        }

        [Test]
        public void CutOut_CompletelyOutside_ReturnsEmptyTransparentPicture()
        {
            Picture picture = Picture.Create(new PictureSize(2, 2), new byte[] {
                10, 20, 30, 40,  50, 60, 70, 80,
                11, 21, 31, 41,  51, 61, 71, 81
            });
            Picture after = picture.CutOut(new PictureArea(new Position(-10, -10), new PictureSize(2, 2)));

            Assert.That(after.Size, Is.EqualTo(new PictureSize(2, 2)));
            Assert.That(after.CloneImage(), Is.All.Zero);
        }

        [Test]
        public void ClearTest()
        {
            Picture picture = Picture.Create(new PictureSize(2, 2), new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8 });
            Picture after = picture.Clear(new PictureArea(new Position(0, 0), new PictureSize(1, 1)));
            Assert.That(after.CloneImage(), Is.EqualTo(new byte[]
            {
                0, 0, 0, 0, 5, 6, 7, 8,
                1, 2, 3, 4, 5, 6, 7, 8
            }));
        }

        [Test]
        public void ApplyTransparencyTest()
        {
            // BGRA layout: { Blue, Green, Red, Alpha }
            byte[] originalPixels = new byte[]
            {
                100, 150, 200, 255, // Pixel 1 (Match)
                50, 60, 70, 255     // Pixel 2 (No match)
            };
            Picture picture = Picture.Create(new PictureSize(2, 1), originalPixels);

            ArgbColor targetColor = new ArgbColor(255, 200, 150, 100);
            Picture after = picture.ApplyTransparency(targetColor);

            Assert.That(after.CloneImage(), Is.EqualTo(new byte[]
            {
                100, 150, 200, 0,   // Alpha becomes 0
                50, 60, 70, 255     // Remains unchanged
            }));
        }

        private Picture CreateSamplePicture(int width, int height, params (int x, int y, ArgbColor color)[] pixels)
        {
            var data = new byte[width * height * 4];
            foreach (var (x, y, color) in pixels)
            {
                int index = (x + y * width) * 4;
                data[index] = color.Blue;
                data[index + 1] = color.Green;
                data[index + 2] = color.Red;
                data[index + 3] = color.Alpha;
            }
            return Picture.Create(new PictureSize(width, height), data);
        }

        [Test]
        public void Create_ThrowsException_WhenImageDataLengthDoesNotMatchTotalBytes()
        {
            var size = new PictureSize(10, 10);
            byte[] invalidData = new byte[10];
            Assert.Throws<ArgumentException>(() => Picture.Create(size, invalidData));
        }

        [Test]
        public void CreateEmpty_AtMaxDimension_CreatesSuccessfully()
        {
            var size = new PictureSize(PictureSize.MaxDimension, 1);
            Picture picture = Picture.CreateEmpty(size);
            Assert.That(picture.Width, Is.EqualTo(PictureSize.MaxDimension));
            Assert.That(picture.Height, Is.EqualTo(1));
            Assert.That(picture.Length, Is.EqualTo(PictureSize.MaxDimension * 4));
        }

        [Test]
        public void ReplaceColor_ReplacesAllMatchingPixelsInWholePicture()
        {
            var colorA = new ArgbColor(255, 255, 0, 0);
            var colorB = new ArgbColor(255, 0, 255, 0);
            var colorC = new ArgbColor(255, 0, 0, 255);
            Picture source = CreateSamplePicture(3, 2,
                (0, 0, colorA), (1, 0, colorB), (2, 0, colorA),
                (0, 1, colorB), (1, 1, colorA), (2, 1, colorB));

            Picture result = source.ReplaceColor(colorA, colorC);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.SameAs(source));
                Assert.That(result.PickColor(new Position(0, 0)), Is.EqualTo(colorC));
                Assert.That(result.PickColor(new Position(1, 0)), Is.EqualTo(colorB));
                Assert.That(result.PickColor(new Position(2, 0)), Is.EqualTo(colorC));
                Assert.That(result.PickColor(new Position(0, 1)), Is.EqualTo(colorB));
                Assert.That(result.PickColor(new Position(1, 1)), Is.EqualTo(colorC));
                Assert.That(result.PickColor(new Position(2, 1)), Is.EqualTo(colorB));
            });
        }

        [Test]
        public void ReplaceColor_WithArea_ReplacesOnlyPixelsInsideArea()
        {
            var colorA = new ArgbColor(255, 255, 0, 0);
            var colorB = new ArgbColor(255, 0, 255, 0);
            Picture source = CreateSamplePicture(3, 3,
                (0, 0, colorA), (1, 0, colorA), (2, 0, colorA),
                (0, 1, colorA), (1, 1, colorA), (2, 1, colorA),
                (0, 2, colorA), (1, 2, colorA), (2, 2, colorA));

            Picture result = source.ReplaceColor(colorA, colorB, new PictureArea(new Position(1, 1), new PictureSize(2, 2)));

            Assert.Multiple(() =>
            {
                Assert.That(result.PickColor(new Position(0, 0)), Is.EqualTo(colorA));
                Assert.That(result.PickColor(new Position(1, 0)), Is.EqualTo(colorA));
                Assert.That(result.PickColor(new Position(0, 1)), Is.EqualTo(colorA));
                Assert.That(result.PickColor(new Position(1, 1)), Is.EqualTo(colorB));
                Assert.That(result.PickColor(new Position(2, 1)), Is.EqualTo(colorB));
                Assert.That(result.PickColor(new Position(1, 2)), Is.EqualTo(colorB));
                Assert.That(result.PickColor(new Position(2, 2)), Is.EqualTo(colorB));
            });
        }

        [Test]
        public void ReplaceColor_WhenTargetColorNotFound_ReturnsSameInstance()
        {
            var colorA = new ArgbColor(255, 255, 0, 0);
            var colorB = new ArgbColor(255, 0, 255, 0);
            var colorC = new ArgbColor(255, 0, 0, 255);
            Picture source = CreateSamplePicture(2, 2, (0, 0, colorA), (1, 0, colorA), (0, 1, colorA), (1, 1, colorA));

            Assert.That(source.ReplaceColor(colorB, colorC), Is.SameAs(source));
        }

        [Test]
        public void ReplaceColor_WhenTargetEqualsReplacement_ReturnsSameInstance()
        {
            var colorA = new ArgbColor(255, 255, 0, 0);
            Picture source = CreateSamplePicture(2, 2, (0, 0, colorA), (1, 0, colorA), (0, 1, colorA), (1, 1, colorA));

            Assert.That(source.ReplaceColor(colorA, colorA), Is.SameAs(source));
        }

        [Test]
        public void CutOut_WhenAreaIsEmpty_ReturnsZeroSizePicture()
        {
            Picture source = CreateSamplePicture(10, 10);
            var emptyArea = new PictureArea(new Position(2, 2), new PictureSize(0, 0));
            Picture result = source.CutOut(emptyArea);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(0));
                Assert.That(result.Height, Is.EqualTo(0));
                Assert.That(result.Length, Is.EqualTo(0));
            });
        }

        [TestCase(20, 20, 5, 5)] // Completely outside (both X and Y)
        [TestCase(2, 20, 5, 5)]  // X overlaps (2..7), Y outside (20..25)
        [TestCase(20, 2, 5, 5)]  // Y overlaps (2..7), X outside (20..25)
        public void CutOut_WhenAreaIsOutside_ReturnsTransparentPicture(int x, int y, int w, int h)
        {
            Picture source = CreateSamplePicture(10, 10);
            var outsideArea = new PictureArea(new Position(x, y), new PictureSize(w, h));
            Picture result = source.CutOut(outsideArea);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(w));
                Assert.That(result.Height, Is.EqualTo(h));
                Assert.That(result.CloneImage(), Is.All.Zero);
            });
        }

        [Test]
        public void Transfer_WithoutMagnification_UsesMagnification1()
        {
            var source = CreateSamplePicture(10, 10);
            var dummyTransfer = new DummyTransfer();
            var result = source.Transfer(dummyTransfer);

            Assert.That(dummyTransfer.ReceivedMagnification?.Value, Is.EqualTo(1));
        }

        private class DummyTransfer : Eede.Domain.ImageEditing.Transformation.IImageTransfer
        {
            public Magnification? ReceivedMagnification { get; private set; }

            public Picture Transfer(Picture src, Magnification magnification)
            {
                ReceivedMagnification = magnification;
                return src;
            }
        }

        [Test]
        public void ApplyTransparency_OnlyExactColorIsMadeTransparent()
        {
            var target = new ArgbColor(255, 100, 150, 200);
            var diffRed = new ArgbColor(255, 99, 150, 200);
            var diffGreen = new ArgbColor(255, 100, 99, 200);
            var diffBlue = new ArgbColor(255, 100, 150, 99);
            var diffAlpha = new ArgbColor(128, 100, 150, 200);

            Picture source = CreateSamplePicture(5, 1,
                (0, 0, target),
                (1, 0, diffRed),
                (2, 0, diffGreen),
                (3, 0, diffBlue),
                (4, 0, diffAlpha));

            Picture result = source.ApplyTransparency(target);

            Assert.Multiple(() =>
            {
                Assert.That(result.PickColor(new Position(0, 0)).Alpha, Is.EqualTo(0), "Target should become transparent");
                Assert.That(result.PickColor(new Position(1, 0)).Alpha, Is.EqualTo(255), "DiffRed should remain opaque");
                Assert.That(result.PickColor(new Position(2, 0)).Alpha, Is.EqualTo(255), "DiffGreen should remain opaque");
                Assert.That(result.PickColor(new Position(3, 0)).Alpha, Is.EqualTo(255), "DiffBlue should remain opaque");
                Assert.That(result.PickColor(new Position(4, 0)).Alpha, Is.EqualTo(128), "DiffAlpha should remain unchanged");
            });
        }

        [Test]
        public void ReplaceColor_ClipsAreaToPictureBounds()
        {
            var red = new ArgbColor(255, 255, 0, 0);
            var blue = new ArgbColor(255, 0, 0, 255);
            Picture source = CreateSamplePicture(2, 2,
                (0, 0, red), (1, 0, red),
                (0, 1, red), (1, 1, red));

            // area は (1, 1) から (5, 5) -> 画像と交差するのは (1, 1) の 1ピクセルのみ
            var area = new PictureArea(new Position(1, 1), new PictureSize(5, 5));
            Picture result = source.ReplaceColor(red, blue, area);

            Assert.Multiple(() =>
            {
                Assert.That(result.PickColor(new Position(0, 0)), Is.EqualTo(red));
                Assert.That(result.PickColor(new Position(1, 0)), Is.EqualTo(red));
                Assert.That(result.PickColor(new Position(0, 1)), Is.EqualTo(red));
                Assert.That(result.PickColor(new Position(1, 1)), Is.EqualTo(blue));
            });
        }

        [TestCase(10, 10, 5, 5)] // completely outside
        [TestCase(0, 10, 5, 5)]  // Y outside
        [TestCase(10, 0, 5, 5)]  // X outside
        public void ReplaceColor_WhenAreaIsOutside_ReturnsSameInstance(int x, int y, int w, int h)
        {
            var red = new ArgbColor(255, 255, 0, 0);
            var blue = new ArgbColor(255, 0, 0, 255);
            Picture source = CreateSamplePicture(2, 2, (0, 0, red), (1, 0, red), (0, 1, red), (1, 1, red));

            var area = new PictureArea(new Position(x, y), new PictureSize(w, h));
            Assert.That(source.ReplaceColor(red, blue, area), Is.SameAs(source));
        }

        [TestCase(0, 0)]
        [TestCase(10, 0)]
        [TestCase(0, 10)]
        public void CutOut_WhenAreaIsEmpty_ReturnsZeroSizeAndEmptyData(int width, int height)
        {
            Picture picture = Picture.Create(new PictureSize(2, 2), new byte[16]);
            Picture result = picture.CutOut(new PictureArea(new Position(1, 1), new PictureSize(width, height)));

            Assert.Multiple(() =>
            {
                Assert.That(result.Size, Is.EqualTo(new PictureSize(0, 0)));
                Assert.That(result.Length, Is.EqualTo(0));
                Assert.That(result.CloneImage(), Is.Empty);
            });
        }

        [TestCase(10, 0)]
        [TestCase(0, 10)]
        [TestCase(-10, 0)]
        [TestCase(0, -10)]
        public void CutOut_WhenAreaIsCompletelyOutsideOnOneAxis_ReturnsTransparentPicture(int x, int y)
        {
            Picture picture = Picture.Create(new PictureSize(2, 2), new byte[] {
                10, 20, 30, 40,  50, 60, 70, 80,
                11, 21, 31, 41,  51, 61, 71, 81
            });
            Picture result = picture.CutOut(new PictureArea(new Position(x, y), new PictureSize(2, 2)));

            Assert.Multiple(() =>
            {
                Assert.That(result.Size, Is.EqualTo(new PictureSize(2, 2)));
                Assert.That(result.CloneImage(), Is.All.Zero);
            });
        }

        [Test]
        public void ReplaceColor_WhenAreaCoversOnlyTopLeft_DoesNotReplaceOtherPixels()
        {
            var red = new ArgbColor(255, 255, 0, 0);
            var blue = new ArgbColor(255, 0, 0, 255);
            // 2x2 全て赤
            Picture source = CreateSamplePicture(2, 2,
                (0, 0, red), (1, 0, red),
                (0, 1, red), (1, 1, red));

            // 左上 (0, 0, 1, 1) のみ対象
            var area = new PictureArea(new Position(0, 0), new PictureSize(1, 1));
            Picture result = source.ReplaceColor(red, blue, area);

            Assert.Multiple(() =>
            {
                Assert.That(result.PickColor(new Position(0, 0)), Is.EqualTo(blue), "Target (0,0) should be replaced with blue");
                Assert.That(result.PickColor(new Position(1, 0)), Is.EqualTo(red), "Outside (1,0) should remain red");
                Assert.That(result.PickColor(new Position(0, 1)), Is.EqualTo(red), "Outside (0,1) should remain red");
                Assert.That(result.PickColor(new Position(1, 1)), Is.EqualTo(red), "Outside (1,1) should remain red");
            });
        }

        [Test]
        public void Create_ThrowsArgumentException_WithDetailedMessage()
        {
            var ex = Assert.Throws<ArgumentException>(() => Picture.Create(new PictureSize(10, 10), new byte[10]));
            Assert.That(ex!.Message, Does.Contain("!= length:10"));
        }
    }
}