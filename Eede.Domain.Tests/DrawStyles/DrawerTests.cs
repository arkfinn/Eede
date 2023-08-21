using Eede.Domain.Colors;
using Eede.Domain.Files;
using Eede.Domain.ImageBlenders;
using Eede.Domain.Pictures;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;

namespace Eede.Domain.DrawStyles
{
    [TestFixture()]
    public class DrawerTests
    {
        [Test()]
        public void DrawPointTest()
        {
            var src = ReadPicture(@"DrawStyles\test\base.png");
            var drawer = new Drawer(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1));
            var dst = drawer.DrawPoint(new Positions.Position(5, 5));
            //dstBmp.Save(@"DrawStyles\test\dest.png", ImageFormat.Png);
            var expected = ReadPicture(@"DrawStyles\test\point1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [Test()]
        public void DrawEllipseTest()
        {
            var src = ReadPicture(@"DrawStyles\test\base.png");
            var drawer = new Drawer(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));
            var dst = drawer.DrawEllipse(new Positions.Position(5, 5), new Positions.Position(10, 10));
            // dstBmp.Save(@"DrawStyles\test\ellipse1.png", ImageFormat.Png);
            var expected = ReadPicture(@"DrawStyles\test\ellipse1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [Test()]
        public void DrawFillEllipseTest()
        {
            var src = ReadPicture(@"DrawStyles\test\base.png");
            var drawer = new Drawer(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));
            var dst = drawer.DrawFillEllipse(new Positions.Position(5, 5), new Positions.Position(10, 10));
            // dstBmp.Save(@"DrawStyles\test\fillEllipse1.png", ImageFormat.Png);
            var expected = ReadPicture(@"DrawStyles\test\fillEllipse1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return new PictureFileReader(new FilePath(path)).Read();
        }
    }
}
