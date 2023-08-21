using Eede.Domain.Colors;
using Eede.Domain.Files;
using Eede.Domain.ImageBlenders;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;

namespace Eede.Domain.DrawStyles
{
    [TestFixture()]
    public class FreeCurveTests
    {
        [Test()]
        public void DrawTest()
        {
            var src = ReadPicture(@"DrawStyles\test\base.png");
            var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
            var drawer = new Drawer(src, penStyle);
            var tool = new FreeCurve();
            var pos = new PositionHistory(new Position(5, 5));
            var process = tool.DrawStart(drawer, pos, false);
            var drawer2 = new Drawer(process, penStyle);
            var pos2 = pos.Update(new Position(8, 4));
            var process2 = tool.Drawing(drawer2, pos2, false);
            var drawer3 = new Drawer(process2, penStyle);
            var pos3 = pos2.Update(new Position(15, 18));
            var process3 = tool.Drawing(drawer3, pos3, false);
            var drawer4 = new Drawer(process3, penStyle);
            var dst = tool.DrawEnd(drawer4, pos3, false);

            //　dstBmp.Save(@"DrawStyles\test\freeCurve1_.png", ImageFormat.Png);
            var expected = ReadPicture(@"DrawStyles\test\freeCurve1.png");
            Assert.That(dst.CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return new PictureFileReader(new FilePath(path)).Read();
        }
    }
}
