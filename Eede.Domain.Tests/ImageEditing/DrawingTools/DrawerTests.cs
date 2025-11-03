using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.Pictures;
using Eede.Domain.SharedKernel;
using Eede.Domain.Tests.Helpers;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools
{
    [TestFixture()]
    public class DrawerTests
    {
        [Test()]
        public void DrawPointTest()
        {
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1));
            Picture dst = drawer.DrawPoint(new Position(5, 5));
            //dstBmp.Save(@"ImageEditing\DrawingTools\test\dest.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"ImageEditing\DrawingTools\test\point1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [TestCase(10, 10, 14, 14, @"ImageEditing\DrawingTools\test\ellipse1.png")]
        [TestCase(10, 10, 15, 15, @"ImageEditing\DrawingTools\test\ellipse2.png")]
        public void DrawEllipseTest(int fx, int fy, int tx, int ty, string expectedImagePath)
        {
            // Arrange
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

            // Act
            Picture dst = drawer.DrawEllipse(new Position(fx, fy), new Position(tx, ty));
            //PictureHelper.WriteBitmap(@"ImageEditing\DrawingTools\test\ellipse_test.png", dst);

            // Assert
            Picture expected = ReadPicture(expectedImagePath);
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [TestCase(5, 5, 9, 9, @"ImageEditing\DrawingTools\test\fillEllipse1.png")]
        [TestCase(5, 5, 10, 10, @"ImageEditing\DrawingTools\test\fillEllipse2.png")]
        public void DrawFillEllipseTest(int fx, int fy, int tx, int ty, string expectedImagePath)
        {
            // Arrange
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

            // Act
            Picture dst = drawer.DrawFillEllipse(new Position(fx, fy), new Position(tx, ty));
            //PictureHelper.WriteBitmap(@"ImageEditing\DrawingTools\test\fillEllipse_test.png", dst);

            // Assert
            Picture expected = ReadPicture(expectedImagePath);
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [Test]
        public void DrawRectangleTest()
        {
            // Arrange
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

            // Act
            Picture dst = drawer.DrawRectangle(new(10, 10), new(15, 15));
            //PictureHelper.WriteBitmap(@"ImageEditing\DrawingTools\test\test.png", dst);

            // Assert
            Picture expected = ReadPicture(@"ImageEditing\DrawingTools\test\rectangle1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [Test]
        public void DrawFillRectangleTest()
        {
            // Arrange
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));

            // Act
            Picture dst = drawer.DrawFillRectangle(new(10, 10), new(15, 15));
            // PictureHelper.WriteBitmap(@"ImageEditing\DrawingTools\test\test.png", dst);

            // Assert
            Picture expected = ReadPicture(@"ImageEditing\DrawingTools\test\fillRectangle1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return PictureHelper.ReadBitmap(path);
        }

    }
}
