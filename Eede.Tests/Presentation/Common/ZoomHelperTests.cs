using System;
using Avalonia;
using Eede.Domain.ImageEditing;
using Eede.Presentation.Common;
using NUnit.Framework;

namespace Eede.Tests.Presentation.Common
{
    [TestFixture]
    public class ZoomHelperTests
    {
        [TestCase(1f, 1, 2f)]
        [TestCase(2f, 1, 4f)]
        [TestCase(4f, 1, 6f)]
        [TestCase(6f, 1, 8f)]
        [TestCase(8f, 1, 12f)]
        [TestCase(12f, 1, 16f)]
        [TestCase(16f, 1, 16f)] // 上限
        [TestCase(3f, 1, 4f)]  // 中間値からの拡大
        public void CalculateNextMagnification_ZoomIn_ReturnsNextStep(float current, int delta, float expected)
        {
            var result = ZoomHelper.CalculateNextMagnification(new Magnification(current), delta);
            Assert.That(result.Value, Is.EqualTo(expected));
        }

        [TestCase(16f, -1, 12f)]
        [TestCase(12f, -1, 8f)]
        [TestCase(8f, -1, 6f)]
        [TestCase(6f, -1, 4f)]
        [TestCase(4f, -1, 2f)]
        [TestCase(2f, -1, 1f)]
        [TestCase(1f, -1, 1f)] // 下限
        [TestCase(3f, -1, 2f)]  // 中間値からの縮小
        public void CalculateNextMagnification_ZoomOut_ReturnsPreviousStep(float current, int delta, float expected)
        {
            var result = ZoomHelper.CalculateNextMagnification(new Magnification(current), delta);
            Assert.That(result.Value, Is.EqualTo(expected));
        }

        [Test]
        public void CalculateNextMagnification_ZeroDelta_ReturnsSame()
        {
            var result = ZoomHelper.CalculateNextMagnification(new Magnification(4f), 0);
            Assert.That(result.Value, Is.EqualTo(4f));
        }

        [Test]
        public void CalculateZoomOffset_ZoomInFromOrigin_ReturnsCorrectOffset()
        {
            // OldOffset = (0, 0), P_view = (100, 100), 1x -> 2x
            // NewOffset = (0 + 100) * (2 / 1) - 100 = 100
            var oldOffset = new Vector(0, 0);
            var pointer = new Point(100, 100);
            var newOffset = ZoomHelper.CalculateZoomOffset(oldOffset, pointer, 1f, 2f);

            Assert.That(newOffset.X, Is.EqualTo(100.0).Within(0.001));
            Assert.That(newOffset.Y, Is.EqualTo(100.0).Within(0.001));
        }

        [Test]
        public void CalculateZoomOffset_ZoomOutWithExistingOffset_ReturnsCorrectOffset()
        {
            // OldOffset = (200, 100), P_view = (50, 50), 4x -> 2x
            // NewOffset = (200 + 50, 100 + 50) * (2 / 4) - (50, 50) = (125 - 50, 75 - 50) = (75, 25)
            var oldOffset = new Vector(200, 100);
            var pointer = new Point(50, 50);
            var newOffset = ZoomHelper.CalculateZoomOffset(oldOffset, pointer, 4f, 2f);

            Assert.That(newOffset.X, Is.EqualTo(75.0).Within(0.001));
            Assert.That(newOffset.Y, Is.EqualTo(25.0).Within(0.001));
        }

        [Test]
        public void CalculateZoomOffset_NegativeOffset_ClampsToZero()
        {
            // OldOffset = (0, 0), P_view = (100, 100), 4x -> 2x
            // NewOffset = (0 + 100) * (2 / 4) - 100 = 50 - 100 = -50 -> 0
            var oldOffset = new Vector(0, 0);
            var pointer = new Point(100, 100);
            var newOffset = ZoomHelper.CalculateZoomOffset(oldOffset, pointer, 4f, 2f);

            Assert.That(newOffset.X, Is.EqualTo(0.0));
            Assert.That(newOffset.Y, Is.EqualTo(0.0));
        }

        [Test]
        public void CalculateZoomOffset_InvalidMag_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ZoomHelper.CalculateZoomOffset(new Vector(0, 0), new Point(0, 0), 0f, 2f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ZoomHelper.CalculateZoomOffset(new Vector(0, 0), new Point(0, 0), 1f, -1f));
        }

        [Test]
        public void CalculatePanOffset_DragRightDown_DecreasesOffset()
        {
            // マウスを右下 (20, 30) へドラッグ -> コンテンツは右下に引っ張られるため Offset は減少
            var startOffset = new Vector(100, 100);
            var delta = new Vector(20, 30);
            var result = ZoomHelper.CalculatePanOffset(startOffset, delta);

            Assert.That(result.X, Is.EqualTo(80.0));
            Assert.That(result.Y, Is.EqualTo(70.0));
        }

        [Test]
        public void CalculatePanOffset_DragLeftUp_IncreasesOffset()
        {
            // マウスを左上 (-20, -30) へドラッグ -> Offset は増加
            var startOffset = new Vector(100, 100);
            var delta = new Vector(-20, -30);
            var result = ZoomHelper.CalculatePanOffset(startOffset, delta);

            Assert.That(result.X, Is.EqualTo(120.0));
            Assert.That(result.Y, Is.EqualTo(130.0));
        }

        [Test]
        public void CalculatePanOffset_NegativeOffset_ClampsToZero()
        {
            var startOffset = new Vector(10, 10);
            var delta = new Vector(50, 50);
            var result = ZoomHelper.CalculatePanOffset(startOffset, delta);

            Assert.That(result.X, Is.EqualTo(0.0));
            Assert.That(result.Y, Is.EqualTo(0.0));
        }

        [Test]
        public void CalculateOverscrollMargin_StandardViewport_ReturnsSnappedMargin()
        {
            // Viewport = (800, 600)
            // W: 800 / 32 = 25 -> 25 * 16 = 400
            // H: 600 / 32 = 18.75 -> ceil(18.75) = 19 -> 19 * 16 = 304
            var viewport = new Size(800, 600);
            var margin = ZoomHelper.CalculateOverscrollMargin(viewport);

            Assert.That(margin.X, Is.EqualTo(400.0));
            Assert.That(margin.Y, Is.EqualTo(304.0));
        }

        [Test]
        public void CalculateOverscrollMargin_TinyViewport_ReturnsMinMargin()
        {
            var viewport = new Size(10, 10);
            var margin = ZoomHelper.CalculateOverscrollMargin(viewport);

            Assert.That(margin.X, Is.EqualTo(32.0));
            Assert.That(margin.Y, Is.EqualTo(32.0));
        }

        [Test]
        public void CalculateZoomOffset_WithMargin_KeepsPointerPositionStatic()
        {
            // Margin = (100, 100), oldOffset = (100, 100), pointer = (50, 50), 1x -> 2x
            // newOffset = 100 + ((100 + 50 - 100) * 2) - 50 = 150
            var oldOffset = new Vector(100, 100);
            var pointer = new Point(50, 50);
            var margin = new Vector(100, 100);
            var newOffset = ZoomHelper.CalculateZoomOffset(oldOffset, pointer, 1f, 2f, margin);

            Assert.That(newOffset.X, Is.EqualTo(150.0).Within(0.001));
            Assert.That(newOffset.Y, Is.EqualTo(150.0).Within(0.001));
        }

        [Test]
        public void CalculateInitialOffset_WhenViewportLarger_CentersCanvasInViewport()
        {
            // エリアのサイズの方が大きいときは中央寄せ
            // margin = (400, 300), displaySize = (200, 100), viewport = (800, 600)
            // initX = 400 + (200 - 800) / 2 = 100
            // initY = 300 + (100 - 600) / 2 = 50
            var margin = new Vector(400, 300);
            var displaySize = new Size(200, 100);
            var viewport = new Size(800, 600);
            var initOffset = ZoomHelper.CalculateInitialOffset(margin, displaySize, viewport);

            Assert.That(initOffset.X, Is.EqualTo(100.0).Within(0.001));
            Assert.That(initOffset.Y, Is.EqualTo(50.0).Within(0.001));
        }

        [Test]
        public void CalculateInitialOffset_WhenDisplayLarger_AlignsOriginToZero()
        {
            // ファイルのサイズの方が大きいときはファイルの(0,0)を座標(0,0)にあわせる（Offset = margin）
            // margin = (400, 300), displaySize = (1000, 800), viewport = (800, 600)
            // initX = margin.X = 400
            // initY = margin.Y = 300
            var margin = new Vector(400, 300);
            var displaySize = new Size(1000, 800);
            var viewport = new Size(800, 600);
            var initOffset = ZoomHelper.CalculateInitialOffset(margin, displaySize, viewport);

            Assert.That(initOffset.X, Is.EqualTo(400.0).Within(0.001));
            Assert.That(initOffset.Y, Is.EqualTo(300.0).Within(0.001));
        }

        [Test]
        public void CalculateInitialOffset_WhenDisplayLargerInOneAxisOnly_HandlesIndependently()
        {
            // 横はファイルが大きく(1000 > 800)、縦はエリアが大きい(400 < 600)場合
            var margin = new Vector(400, 300);
            var displaySize = new Size(1000, 400);
            var viewport = new Size(800, 600);
            var initOffset = ZoomHelper.CalculateInitialOffset(margin, displaySize, viewport);

            Assert.That(initOffset.X, Is.EqualTo(400.0).Within(0.001)); // ファイル0,0合わせ
            Assert.That(initOffset.Y, Is.EqualTo(200.0).Within(0.001)); // 300 + (400 - 600)/2 = 200 (中央寄せ)
        }
    }
}
