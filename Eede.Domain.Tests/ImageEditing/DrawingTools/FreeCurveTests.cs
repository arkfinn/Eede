using Eede.Domain.ImageEditing;
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
    public class FreeCurveTests
    {
        [Test()]
        public void DrawTest()
        {
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
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

            //　dstBmp.Save(@"ImageEditing\DrawingTools\test\freeCurve1_.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"ImageEditing\DrawingTools\test\freeCurve1.png");
            Assert.That(dst.Fetch().CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        private Picture ReadPicture(string path)
        {
            return PictureHelper.ReadBitmap(path);
        }
    }
}
