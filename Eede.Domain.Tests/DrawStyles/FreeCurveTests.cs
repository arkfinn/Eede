using Eede.Domain.Colors;
using Eede.Domain.Drawings;
using Eede.Domain.DrawStyles;
using Eede.Domain.Files;
using Eede.Domain.ImageBlenders;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;

namespace Eede.Domain.Tests.DrawStyles
{
    [TestFixture()]
    public class FreeCurveTests
    {
        [Test()]
        public void DrawTest()
        {
            Picture src = ReadPicture(@"DrawStyles\test\base.png");
            DrawingBuffer buffer = new(src);
            PenStyle penStyle = new(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
            FreeCurve tool = new();
            PositionHistory pos = new(new Position(5, 5));
            DrawingBuffer process = tool.DrawStart(buffer, penStyle, pos, false);
            PositionHistory pos2 = pos.Update(new Position(8, 4));
            DrawingBuffer process2 = tool.Drawing(process, penStyle, pos2, false);
            PositionHistory pos3 = pos2.Update(new Position(15, 18));
            DrawingBuffer process3 = tool.Drawing(process2, penStyle, pos3, false);
            DrawingBuffer dst = tool.DrawEnd(process3, penStyle, pos3, false);

            //　dstBmp.Save(@"DrawStyles\test\freeCurve1_.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"DrawStyles\test\freeCurve1.png");
            Assert.That(dst.Fetch().CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return new PictureFileReader(new FilePath(path)).Read();
        }
    }
}
