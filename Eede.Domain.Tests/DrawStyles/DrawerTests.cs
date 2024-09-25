using Eede.Domain.Colors;
using Eede.Domain.DrawStyles;
using Eede.Domain.Files;
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

        [Test()]
        public void DrawEllipseTest()
        {
            Picture src = ReadPicture(@"DrawStyles\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));
            Picture dst = drawer.DrawEllipse(new Position(5, 5), new Position(10, 10));
            // dstBmp.Save(@"DrawStyles\test\ellipse1.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"DrawStyles\test\ellipse1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [Test()]
        public void DrawFillEllipseTest()
        {
            Picture src = ReadPicture(@"DrawStyles\test\base.png");
            Drawer drawer = new(src, new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1));
            Picture dst = drawer.DrawFillEllipse(new Position(5, 5), new Position(10, 10));
            // dstBmp.Save(@"DrawStyles\test\fillEllipse1.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"DrawStyles\test\fillEllipse1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return PictureHelper.ReadBitmap(path);
        }
    }
}
