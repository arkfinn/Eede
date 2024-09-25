using Eede.Application.Pictures;
using Eede.Domain.Colors;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using NUnit.Framework;
using System;
using System.Drawing;

namespace Eede.Domain.Pictures
{
    [TestFixture]
    public class PictureTests
    {
        //[Test]
        //public void 引数imageがnullでnewはできない()
        //{
        //    Assert.Throws<ArgumentNullException>(() =>
        //    {
        //        var p = new Picture(null);
        //    });
        //}

        // TODO
        //[TestCaseSource(nameof(PickColorCases))]
        //public void PickColorTest(int x, int y, Color expected)
        //{
        //    using Bitmap b = new(10, 10);
        //    b.SetPixel(1, 2, Color.DarkSeaGreen);
        //    b.SetPixel(2, 3, Color.Beige);
        //    ArgbColor color = BitmapConverter.ConvertBack(b).PickColor(new Position(x, y));
        //    Assert.That(
        //        Tuple.Create(color.Alpha, color.Red, color.Green, color.Blue),
        //        Is.EqualTo(Tuple.Create(expected.A, expected.R, expected.G, expected.B)));
        //}

        //private static readonly object[] PickColorCases =
        //{
        //    new object[] { 1, 2, Color.DarkSeaGreen },
        //    new object[] { 2, 3, Color.Beige },
        //};

        [Test]
        public void PickColorの引数posはbitmapの範囲外を許容しない()
        {
            _ = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var source = Picture.Create(new PictureSize(10, 10), new byte[10 * 4 * 10]);
                _ = source.PickColor(new Position(10, 10));
            });
        }

        //TODO
        //[TestCaseSource(nameof(CutOutCases))]
        //public void CutOutTest(int x, int y, Color expected)
        //{
        //    using Bitmap b = new(10, 10);
        //    b.SetPixel(1, 2, Color.DarkSeaGreen);
        //    b.SetPixel(2, 3, Color.Beige);
        //    Picture source = BitmapConverter.ConvertBack(b);

        //    Picture d = source.CutOut(new PictureArea(new Position(1, 2), new PictureSize(5, 6)));

        //    ArgbColor color = d.PickColor(new Position(x, y));
        //    Assert.That(
        //        Tuple.Create(color.Alpha, color.Red, color.Green, color.Blue),
        //        Is.EqualTo(Tuple.Create(expected.A, expected.R, expected.G, expected.B)));
        //}

        private static readonly object[] CutOutCases =
        {
            new object[] { 0, 0, Color.DarkSeaGreen },
            new object[] { 1, 1, Color.Beige },
        };

        [Test]
        public void CutOutErrorTest()
        {
            // 画像自体のサイズより大きいカットサイズを指定する場合、余りは空白で埋める
            var picture = Picture.Create(new PictureSize(2, 2), new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8 });
            var after = picture.CutOut(new PictureArea(new Position(0, 0), new PictureSize(4, 4)));
            Assert.That(after.CloneImage(), Is.EqualTo(new byte[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 0, 0, 0, 0, 0, 0, 0, 0,
                1, 2, 3, 4, 5, 6, 7, 8, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            }));

        }
    }
}