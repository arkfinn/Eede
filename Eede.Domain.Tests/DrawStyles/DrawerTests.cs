using Eede.Domain.Colors;
using Eede.Domain.DrawStyles;
using Eede.Domain.ImageBlenders;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.DrawStyles
{
    [TestFixture()]
    public class DrawerTests
    {
        [Test()]
        public void DrawPointTest()
        {
            Picture src = ReadPicture(@"DrawStyles\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1));
            Picture dst = drawer.DrawPoint(new Position(5, 5));
            //dstBmp.Save(@"DrawStyles\test\dest.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"DrawStyles\test\point1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [TestCase(10, 10, 14, 14, @"DrawStyles\test\ellipse1.png")]
        [TestCase(10, 10, 15, 15, @"DrawStyles\test\ellipse2.png")]
        public void DrawEllipseTest(int fx, int fy, int tx, int ty, string expectedImagePath)
        {
            // Arrange
            Picture src = ReadPicture(@"DrawStyles\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

            // Act
            Picture dst = drawer.DrawEllipse(new Position(fx, fy), new Position(tx, ty));
            //PictureHelper.WriteBitmap(@"DrawStyles\test\ellipse_test.png", dst);

            // Assert
            Picture expected = ReadPicture(expectedImagePath);
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [TestCase(5, 5, 9, 9, @"DrawStyles\test\fillEllipse1.png")]
        [TestCase(5, 5, 10, 10, @"DrawStyles\test\fillEllipse2.png")]
        public void DrawFillEllipseTest(int fx, int fy, int tx, int ty, string expectedImagePath)
        {
            // Arrange
            Picture src = ReadPicture(@"DrawStyles\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

            // Act
            Picture dst = drawer.DrawFillEllipse(new Position(fx, fy), new Position(tx, ty));
            //PictureHelper.WriteBitmap(@"DrawStyles\test\fillEllipse_test.png", dst);

            // Assert
            Picture expected = ReadPicture(expectedImagePath);
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [Test]
        public void DrawRectangleTest()
        {
            // Arrange
            Picture src = ReadPicture(@"DrawStyles\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

            // Act
            Picture dst = drawer.DrawRectangle(new(10, 10), new(15, 15));
            //PictureHelper.WriteBitmap(@"DrawStyles\test\test.png", dst);

            // Assert
            Picture expected = ReadPicture(@"DrawStyles\test\rectangle1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [Test]
        public void DrawFillRectangleTest()
        {
            // Arrange
            Picture src = ReadPicture(@"DrawStyles\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

            // Act
            Picture dst = drawer.DrawFillRectangle(new(10, 10), new(15, 15));
            // PictureHelper.WriteBitmap(@"DrawStyles\test\test.png", dst);

            // Assert
            Picture expected = ReadPicture(@"DrawStyles\test\fillRectangle1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return PictureHelper.ReadBitmap(path);
        }

    }
}
