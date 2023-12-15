using Eede.Application.Pictures;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using NUnit.Framework;
using System;
using System.Drawing;

namespace Eede.Domain.Tests.Pictures
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

        [TestCaseSource(nameof(PickColorCases))]
        public void PickColorTest(int x, int y, Color expected)
        {
            using Bitmap b = new(10, 10);
            b.SetPixel(1, 2, Color.DarkSeaGreen);
            b.SetPixel(2, 3, Color.Beige);
            Colors.ArgbColor color = BitmapConverter.ConvertBack(b).PickColor(new Position(x, y));
            Assert.That(
                Tuple.Create(color.Alpha, color.Red, color.Green, color.Blue),
                Is.EqualTo(Tuple.Create(expected.A, expected.R, expected.G, expected.B)));
        }

        private static readonly object[] PickColorCases =
        {
            new object[] { 1, 2, Color.DarkSeaGreen },
            new object[] { 2, 3, Color.Beige },
        };

        [Test]
        public void PickColorの引数posはbitmapの範囲外を許容しない()
        {
            _ = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using Bitmap b = new(10, 10);
                _ = BitmapConverter.ConvertBack(b).PickColor(new Position(10, 10));
            });
        }

        [TestCaseSource(nameof(CutOutCases))]
        public void CutOutTest(int x, int y, Color expected)
        {
            using Bitmap b = new(10, 10);
            b.SetPixel(1, 2, Color.DarkSeaGreen);
            b.SetPixel(2, 3, Color.Beige);
            Picture d = BitmapConverter.ConvertBack(b).CutOut(new PictureArea(new Position(1, 2), new PictureSize(5, 6)));
            Colors.ArgbColor color = d.PickColor(new Position(x, y));
            Assert.That(
                Tuple.Create(color.Alpha, color.Red, color.Green, color.Blue),
                Is.EqualTo(Tuple.Create(expected.A, expected.R, expected.G, expected.B)));
        }

        private static readonly object[] CutOutCases =
        {
            new object[] { 0, 0, Color.DarkSeaGreen },
            new object[] { 1, 1, Color.Beige },
        };
    }
}