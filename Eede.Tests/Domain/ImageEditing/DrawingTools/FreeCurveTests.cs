using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
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
            CoordinateHistory pos = new(new CanvasCoordinate(5, 5));
            DrawingBuffer process = tool.DrawStart(buffer, penStyle, pos, false);
            CoordinateHistory pos2 = pos.Update(new CanvasCoordinate(8, 4));
            DrawingBuffer process2 = tool.Drawing(process, penStyle, pos2, false);
            CoordinateHistory pos3 = pos2.Update(new CanvasCoordinate(15, 18));
            DrawingBuffer process3 = tool.Drawing(process2, penStyle, pos3, false);
            var result = tool.DrawEnd(process3, penStyle, pos3, false);

            //　dstBmp.Save(@"ImageEditing\DrawingTools\test\freeCurve1_.png", ImageFormat.Png);
            Picture expected = ReadPicture(@"ImageEditing\DrawingTools\test\freeCurve1.png");
            Assert.That(result.Buffer.Fetch().CloneImage(), Is.EqualTo(expected.CloneImage()));
        }

        [Test]
        public void DrawStart_ClearsPreviousAffectedAreas()
        {
            // Arrange
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            DrawingBuffer buffer = new(src);
            PenStyle penStyle = new(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
            FreeCurve tool = new();

            // 1回目のストローク: (5, 5) から (10, 10)
            CoordinateHistory pos1 = new(new CanvasCoordinate(5, 5));
            var b1 = tool.DrawStart(buffer, penStyle, pos1, false);
            var pos2 = pos1.Update(new CanvasCoordinate(10, 10));
            var b2 = tool.Drawing(b1, penStyle, pos2, false);
            tool.DrawEnd(b2, penStyle, pos2, false);

            // 2回目のストローク: (20, 20) のみ
            CoordinateHistory pos3 = new(new CanvasCoordinate(20, 20));
            var b3 = tool.DrawStart(buffer, penStyle, pos3, false);
            var endResult = tool.DrawEnd(b3, penStyle, pos3, false);

            // Assert: 2回目の AffectedArea は (20, 20) のみであり、前回の (5, 5)〜(10, 10) が残っていない
            var boundingBox = endResult.AffectedArea.GetBoundingBox();
            Assert.Multiple(() =>
            {
                Assert.That(boundingBox.X, Is.EqualTo(20));
                Assert.That(boundingBox.Y, Is.EqualTo(20));
                Assert.That(boundingBox.Width, Is.EqualTo(1));
                Assert.That(boundingBox.Height, Is.EqualTo(1));
            });
        }

        [Test]
        public void Drawing_WhenNowIsOutsideAndLastIsInside_DrawsLine()
        {
            // Arrange: 32x32 のキャンバス
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            DrawingBuffer buffer = new(src);
            PenStyle penStyle = new(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
            FreeCurve tool = new();

            // Act: キャンバス内 (10, 10) からキャンバス外 (-5, 10) へドラッグ
            CoordinateHistory startPos = new(new CanvasCoordinate(10, 10));
            var b1 = tool.DrawStart(buffer, penStyle, startPos, false);
            var outPos = startPos.Update(new CanvasCoordinate(-5, 10));
            var b2 = tool.Drawing(b1, penStyle, outPos, false);
            var endResult = tool.DrawEnd(b2, penStyle, outPos, false);

            // Assert: 例外なく描画され、(-5, 10) から (10, 10) までの線が影響領域に含まれる
            Assert.That(endResult.AffectedArea.IsEmpty, Is.False);
            var boundingBox = endResult.AffectedArea.GetBoundingBox();
            Assert.Multiple(() =>
            {
                Assert.That(boundingBox.X, Is.EqualTo(-5));
                Assert.That(boundingBox.Y, Is.EqualTo(10));
                Assert.That(boundingBox.Width, Is.EqualTo(16)); // 10 - (-5) + 1 = 16
            });
        }

        [Test]
        public void Drawing_WhenBothOutside_ReturnsEmptySegmentArea()
        {
            // Arrange
            Picture src = ReadPicture(@"ImageEditing\DrawingTools\test\base.png");
            DrawingBuffer buffer = new(src);
            PenStyle penStyle = new(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
            FreeCurve tool = new();

            // Act: キャンバス外 (-10, -10) から (-5, -5) へ移動
            CoordinateHistory startPos = new(new CanvasCoordinate(-10, -10));
            var b1 = tool.DrawStart(buffer, penStyle, startPos, false);
            var outPos = startPos.Update(new CanvasCoordinate(-5, -5));
            var b2 = tool.Drawing(b1, penStyle, outPos, false);
            var endResult = tool.DrawEnd(b2, penStyle, outPos, false);

            // Assert: DrawStart(-10, -10) の 1x1 のみで、Drawing(-5, -5) からの線分は追加されない
            var boundingBox = endResult.AffectedArea.GetBoundingBox();
            Assert.Multiple(() =>
            {
                Assert.That(boundingBox.X, Is.EqualTo(-10));
                Assert.That(boundingBox.Y, Is.EqualTo(-10));
                Assert.That(boundingBox.Width, Is.EqualTo(1));
                Assert.That(boundingBox.Height, Is.EqualTo(1));
            });
        }

        private Picture ReadPicture(string path)
        {
            return PictureHelper.ReadBitmap(path);
        }
    }
}
