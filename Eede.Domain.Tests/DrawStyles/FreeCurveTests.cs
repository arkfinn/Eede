using Eede.Domain.Colors;
using Eede.Domain.Drawings;
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
            var buffer = new DrawingBuffer(src);
            var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
            var tool = new FreeCurve();
            var pos = new PositionHistory(new Position(5, 5));
            var process = tool.DrawStart(buffer, penStyle, pos, false);
            var pos2 = pos.Update(new Position(8, 4));
            var process2 = tool.Drawing(process, penStyle, pos2, false);
            var pos3 = pos2.Update(new Position(15, 18));
            var process3 = tool.Drawing(process2, penStyle, pos3, false);
            var dst = tool.DrawEnd(process3, penStyle, pos3, false);

            //　dstBmp.Save(@"DrawStyles\test\freeCurve1_.png", ImageFormat.Png);
            var expected = ReadPicture(@"DrawStyles\test\freeCurve1.png");
            Assert.That(dst.Fetch().CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return new PictureFileReader(new FilePath(path)).Read();
        }
    }
}
