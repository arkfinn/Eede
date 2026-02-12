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
    }
}