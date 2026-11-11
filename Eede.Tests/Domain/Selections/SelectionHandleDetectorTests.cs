using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.Selections
{
    [TestFixture]
    public class SelectionHandleDetectorTests
    {
        [Test]
        public void Detect_AllHandles_DetectedCorrectly()
        {
            var area = new PictureArea(new Position(100, 100), new PictureSize(100, 100));
            int handleSize = 10;

            Assert.Multiple(() =>
            {
                // 四隅 (area.X, area.Y) 〜 (area.X + Width, area.Y + Height)
                Assert.That(SelectionHandleDetector.Detect(area, new Position(100, 100), handleSize), Is.EqualTo(SelectionHandle.TopLeft));
                Assert.That(SelectionHandleDetector.Detect(area, new Position(200, 100), handleSize), Is.EqualTo(SelectionHandle.TopRight));
                Assert.That(SelectionHandleDetector.Detect(area, new Position(100, 200), handleSize), Is.EqualTo(SelectionHandle.BottomLeft));
                Assert.That(SelectionHandleDetector.Detect(area, new Position(200, 200), handleSize), Is.EqualTo(SelectionHandle.BottomRight));

                // 四辺
                Assert.That(SelectionHandleDetector.Detect(area, new Position(150, 100), handleSize), Is.EqualTo(SelectionHandle.Top));
                Assert.That(SelectionHandleDetector.Detect(area, new Position(150, 200), handleSize), Is.EqualTo(SelectionHandle.Bottom));
                Assert.That(SelectionHandleDetector.Detect(area, new Position(100, 150), handleSize), Is.EqualTo(SelectionHandle.Left));
                Assert.That(SelectionHandleDetector.Detect(area, new Position(200, 150), handleSize), Is.EqualTo(SelectionHandle.Right));

                // 中央 (ヒットなし)
                Assert.That(SelectionHandleDetector.Detect(area, new Position(150, 150), handleSize), Is.Null);
            });
        }

        [Test]
        public void Detect_WhenAreaIsSmall_AdjustsHandleSize()
        {
            // minDimension = Math.Min(30, 90) = 30
            // handleSize = 20 -> handleSize * 2 = 40
            // minDimension * 2 / 3 = 20
            // 40 > 20 が成立するため adjustedHandleSize = 30 / 3 = 10 (tolerance = 5)
            // もし Math.Max に変異した場合 minDimension = 90 となり制限がかからない
            var area = new PictureArea(new Position(100, 100), new PictureSize(30, 90));
            int handleSize = 20;

            // TopLeft は (100, 100)。tolerance=5 なので X=106 は外れ
            Assert.Multiple(() =>
            {
                Assert.That(SelectionHandleDetector.Detect(area, new Position(105, 100), handleSize), Is.EqualTo(SelectionHandle.TopLeft));
                Assert.That(SelectionHandleDetector.Detect(area, new Position(106, 100), handleSize), Is.Null);
            });
        }

        [Test]
        public void Detect_HandleSizeBoundaryCondition()
        {
            var area = new PictureArea(new Position(100, 100), new PictureSize(60, 60));

            // minDimension / 3 = 20.
            // handleSize = 20 は 20 > 20 が false なので非調整 (size = 20, tolerance = 10)
            // handleSize = 25 は 25 > 20 が true なので調整 (size = 20, tolerance = 10)
            Assert.Multiple(() =>
            {
                Assert.That(SelectionHandleDetector.Detect(area, new Position(110, 100), 20), Is.EqualTo(SelectionHandle.TopLeft));
                Assert.That(SelectionHandleDetector.Detect(area, new Position(111, 100), 20), Is.Null);

                Assert.That(SelectionHandleDetector.Detect(area, new Position(110, 100), 25), Is.EqualTo(SelectionHandle.TopLeft));
                Assert.That(SelectionHandleDetector.Detect(area, new Position(111, 100), 25), Is.Null);
            });
        }
    }
}
