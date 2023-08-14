using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using NUnit.Framework;
using System.Drawing;
using System.Drawing.Imaging;

namespace Eede.Domain.DrawStyles
{
    [TestFixture()]
    public class DrawerTests
    {
        [Test()]
        public void DrawPointTest()
        {
            var srcBmp = new Bitmap(@"DrawStyles\test\base.png");
            var src = new Picture(srcBmp);
            var drawer = new Drawer(src, new PenStyle(new DirectImageBlender(), Color.Black, 1));
            var dst = drawer.DrawPoint(new Positions.Position(5, 5));
            var dstBmp = dst.ToImage();
            //dstBmp.Save(@"DrawStyles\test\dest.png", ImageFormat.Png);
            var expected = new Bitmap(@"DrawStyles\test\point1.png");
            Assert.IsTrue(ImageComparer.Equals(dstBmp, expected));
        }

        [Test()]
        public void DrawEllipseTest()
        {
            var srcBmp = new Bitmap(@"DrawStyles\test\base.png");
            var src = new Picture(srcBmp);
            var drawer = new Drawer(src, new PenStyle(new DirectImageBlender(), Color.Red, 1));
            var dst = drawer.DrawEllipse(new Positions.Position(5, 5), new Positions.Position(10, 10));
            var dstBmp = dst.ToImage();
            // dstBmp.Save(@"DrawStyles\test\ellipse1.png", ImageFormat.Png);
            var expected = new Bitmap(@"DrawStyles\test\ellipse1.png");
            Assert.IsTrue(ImageComparer.Equals(dstBmp, expected));
        }

        [Test()]
        public void DrawFillEllipseTest()
        {
            var srcBmp = new Bitmap(@"DrawStyles\test\base.png");
            var src = new Picture(srcBmp);
            var drawer = new Drawer(src, new PenStyle(new DirectImageBlender(), Color.Red, 1));
            var dst = drawer.DrawFillEllipse(new Positions.Position(5, 5), new Positions.Position(10, 10));
            var dstBmp = dst.ToImage();
            dstBmp.Save(@"DrawStyles\test\fillEllipse1.png", ImageFormat.Png);
            var expected = new Bitmap(@"DrawStyles\test\fillEllipse1.png");
            Assert.IsTrue(ImageComparer.Equals(dstBmp, expected));
        }
    }
}
