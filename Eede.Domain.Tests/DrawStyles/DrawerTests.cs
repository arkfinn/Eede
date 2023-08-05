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
            var dst = drawer.DrawPoint(new Positions.Position(new Point(5, 5)));
            var dstBmp = dst.ToImage();
            //dstBmp.Save(@"DrawStyles\test\dest.png", ImageFormat.Png);
            var expected = new Bitmap(@"DrawStyles\test\point1.png");
            Assert.IsTrue(ImageComparer.Equals(dstBmp, expected));
        }
    }
}
