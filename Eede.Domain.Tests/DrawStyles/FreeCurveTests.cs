using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using NUnit.Framework;
using System.Drawing;
using System.Drawing.Imaging;

namespace Eede.Domain.DrawStyles
{
    [TestFixture()]
    public class FreeCurveTests
    {
        [Test()]
        public void DrawTest()
        {
            var srcBmp = new Bitmap(@"DrawStyles\test\base.png");
            var src = new Picture(srcBmp);
            var drawer = new Drawer(src, new PenStyle(new DirectImageBlender(), Color.Black, 1));
            var tool = new FreeCurve();
            var pos = new PositionHistory(new Position(5, 5));
            var process = tool.DrawStart(drawer, pos, false);
            var drawer2 = new Drawer(process, new PenStyle(new DirectImageBlender(), Color.Black, 1));
            var pos2 = pos.Update(new Position(8, 4));
            var process2 = tool.Drawing(drawer2, pos2, false);
            var drawer3 = new Drawer(process2, new PenStyle(new DirectImageBlender(), Color.Black, 1));
            var pos3 = pos2.Update(new Position(15, 18));
            var process3 = tool.Drawing(drawer3, pos3, false);
            var drawer4 = new Drawer(process3, new PenStyle(new DirectImageBlender(), Color.Black, 1));
            var dst = tool.DrawEnd(drawer4, pos3, false);

            var dstBmp = dst.ToImage();
            dstBmp.Save(@"DrawStyles\test\freeCurve1_.png", ImageFormat.Png);
            var expected = new Bitmap(@"DrawStyles\test\freeCurve1.png");
            Assert.IsTrue(ImageComparer.Equals(dstBmp, expected));
        }
    }
}
