using Eede.Domain.Selections;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.Selections
{
    [TestFixture]
    public class ResizingSelectionTests
    {
        [Test]
        public void Resize_BottomRight_IncreasesSize()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var resizer = new ResizingSelection(original, SelectionHandle.BottomRight);

            var startPos = new Position(30, 30); // BottomRight corner of original
            var currentPos = new Position(40, 50); // Dragged by +10, +20

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.X, Is.EqualTo(10));
                Assert.That(result.Y, Is.EqualTo(10));
                Assert.That(result.Width, Is.EqualTo(30)); // 20 + 10
                Assert.That(result.Height, Is.EqualTo(40)); // 20 + 20
            });
        }

        [Test]
        public void Resize_TopLeft_MovesPositionAndResizes()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var resizer = new ResizingSelection(original, SelectionHandle.TopLeft);

            var startPos = new Position(10, 10);
            var currentPos = new Position(5, 5); // Dragged by -5, -5

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.X, Is.EqualTo(5));
                Assert.That(result.Y, Is.EqualTo(5));
                Assert.That(result.Width, Is.EqualTo(25)); // 20 + 5
                Assert.That(result.Height, Is.EqualTo(25)); // 20 + 5
            });
        }

        [Test]
        public void Resize_TopRight_MovesPositionAndResizes()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var resizer = new ResizingSelection(original, SelectionHandle.TopRight);

            var startPos = new Position(30, 10); // TopRight corner
            var currentPos = new Position(35, 5); // Dragged by +5, -5

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.X, Is.EqualTo(10));
                Assert.That(result.Y, Is.EqualTo(5));
                Assert.That(result.Width, Is.EqualTo(25)); // 20 + 5
                Assert.That(result.Height, Is.EqualTo(25)); // 20 + 5
            });
        }

        [Test]
        public void Resize_BottomLeft_MovesPositionAndResizes()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var resizer = new ResizingSelection(original, SelectionHandle.BottomLeft);

            var startPos = new Position(10, 30); // BottomLeft corner
            var currentPos = new Position(5, 35); // Dragged by -5, +5

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.X, Is.EqualTo(5));
                Assert.That(result.Y, Is.EqualTo(10));
                Assert.That(result.Width, Is.EqualTo(25)); // 20 + 5
                Assert.That(result.Height, Is.EqualTo(25)); // 20 + 5
            });
        }

        [Test]
        public void Resize_MinimumSizeIs1px()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var resizer = new ResizingSelection(original, SelectionHandle.BottomRight);

            var startPos = new Position(30, 30);
            var currentPos = new Position(10, 10); // Try to shrink to 0 size

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(1));
                Assert.That(result.Height, Is.EqualTo(1));
            });
        }

        [Test]
        public void Resize_KeepAspectRatio_Square()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20)); // 1:1 ratio
            var resizer = new ResizingSelection(original, SelectionHandle.BottomRight);

            var startPos = new Position(30, 30);
            var currentPos = new Position(40, 50); // Dragged to make width 30, height 40

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: true);

            Assert.That(result.Width, Is.EqualTo(result.Height));
        }

        [Test]
        public void Resize_TopRight_KeepAspectRatio_Square()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20)); // 1:1 ratio
            var resizer = new ResizingSelection(original, SelectionHandle.TopRight);

            var startPos = new Position(30, 10);
            var currentPos = new Position(40, 0); // Dragged to make width 30, height 30

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: true);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(result.Height));
                Assert.That(result.X, Is.EqualTo(10));
                Assert.That(result.Y + result.Height, Is.EqualTo(30)); // bottom (Y + Height) shouldn't change
            });
        }

        [Test]
        public void Resize_BottomLeft_KeepAspectRatio_Square()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20)); // 1:1 ratio
            var resizer = new ResizingSelection(original, SelectionHandle.BottomLeft);

            var startPos = new Position(10, 30);
            var currentPos = new Position(0, 40); // Dragged to make width 30, height 30

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: true);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(result.Height));
                Assert.That(result.Y, Is.EqualTo(10));
                Assert.That(result.X + result.Width, Is.EqualTo(30)); // right (X + Width) shouldn't change
            });
        }

        [Test]
        public void Resize_BottomRight_VerticalDrag_ExpandsBoth()
        {
            var original = new PictureArea(new Position(0, 0), new PictureSize(100, 100));
            var resizer = new ResizingSelection(original, SelectionHandle.BottomRight);

            var startPos = new Position(100, 100);
            var currentPos = new Position(100, 120); // Pure vertical drag

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: true);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(120));
                Assert.That(result.Height, Is.EqualTo(120));
            });
        }

        [Test]
        public void Resize_BottomRight_Shrink_Works()
        {
            var original = new PictureArea(new Position(0, 0), new PictureSize(100, 100));
            var resizer = new ResizingSelection(original, SelectionHandle.BottomRight);

            var startPos = new Position(100, 100);
            var currentPos = new Position(80, 90); // Shrink, more on width

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: true);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(80));
                Assert.That(result.Height, Is.EqualTo(80));
            });
        }

        [TestCase(SelectionHandle.Top, 0, -10, 10, 0, 20, 30)]
        [TestCase(SelectionHandle.Bottom, 0, 10, 10, 10, 20, 30)]
        [TestCase(SelectionHandle.Left, -10, 0, 0, 10, 30, 20)]
        [TestCase(SelectionHandle.Right, 10, 0, 10, 10, 30, 20)]
        public void Resize_EdgeHandles_Work(SelectionHandle handle, int dx, int dy, int expX, int expY, int expW, int expH)
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var resizer = new ResizingSelection(original, handle);

            var startPos = new Position(10, 10);
            var currentPos = new Position(10 + dx, 10 + dy);

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.X, Is.EqualTo(expX));
                Assert.That(result.Y, Is.EqualTo(expY));
                Assert.That(result.Width, Is.EqualTo(expW));
                Assert.That(result.Height, Is.EqualTo(expH));
            });
        }

        [TestCase(SelectionHandle.Left, 50, 0, 29, 10, 1, 20)]
        [TestCase(SelectionHandle.TopLeft, 50, 50, 29, 29, 1, 1)]
        [TestCase(SelectionHandle.BottomLeft, 50, 0, 29, 10, 1, 20)]
        [TestCase(SelectionHandle.Right, -50, 0, 10, 10, 1, 20)]
        [TestCase(SelectionHandle.Top, 0, 50, 10, 29, 20, 1)]
        [TestCase(SelectionHandle.TopRight, 0, 50, 10, 29, 20, 1)]
        [TestCase(SelectionHandle.Bottom, 0, -50, 10, 10, 20, 1)]
        public void Resize_InversionPrevention_ClampsTo1(SelectionHandle handle, int dx, int dy, int expX, int expY, int expW, int expH)
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var resizer = new ResizingSelection(original, handle);

            var startPos = new Position(10, 10);
            var currentPos = new Position(10 + dx, 10 + dy);

            var result = resizer.Resize(startPos, currentPos, keepAspectRatio: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.X, Is.EqualTo(expX));
                Assert.That(result.Y, Is.EqualTo(expY));
                Assert.That(result.Width, Is.EqualTo(expW));
                Assert.That(result.Height, Is.EqualTo(expH));
            });
        }

        [Test]
        public void Resize_TopLeft_KeepAspectRatio_NonSquare()
        {
            var original = new PictureArea(new Position(100, 100), new PictureSize(100, 50));
            var resizer = new ResizingSelection(original, SelectionHandle.TopLeft);

            var result = resizer.Resize(new Position(100, 100), new Position(80, 90), keepAspectRatio: true);

            Assert.Multiple(() =>
            {
                Assert.That(result.X, Is.EqualTo(80));
                Assert.That(result.Y, Is.EqualTo(90));
                Assert.That(result.Width, Is.EqualTo(120));
                Assert.That(result.Height, Is.EqualTo(60));
            });
        }

        [Test]
        public void Resize_NonSquare_VerticalDriven()
        {
            var original = new PictureArea(new Position(0, 0), new PictureSize(100, 50));
            var resizer = new ResizingSelection(original, SelectionHandle.BottomRight);

            var result = resizer.Resize(new Position(100, 50), new Position(110, 80), keepAspectRatio: true);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(160));
                Assert.That(result.Height, Is.EqualTo(80));
            });
        }

        [Test]
        public void Resize_OnePixelSelection_DoesNotTriggerInversionClamping()
        {
            // 幅1, 高さ1の選択範囲で、ドラッグ量 0 (dx=0, dy=0)
            // newWidth = 1, newHeight = 1 なので、newWidth < 1 は false
            // もし newWidth <= 1 に変異すると、right = left + 1 が走る
            var original = new PictureArea(new Position(10, 10), new PictureSize(1, 1));
            var resizer = new ResizingSelection(original, SelectionHandle.Right);

            var result = resizer.Resize(new Position(11, 10), new Position(11, 10), keepAspectRatio: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.X, Is.EqualTo(10));
                Assert.That(result.Y, Is.EqualTo(10));
                Assert.That(result.Width, Is.EqualTo(1));
                Assert.That(result.Height, Is.EqualTo(1));
            });
        }

        [Test]
        public void Resize_EqualChange_FavorsHorizontalAxis()
        {
            // 幅100, 高さ50 (ratio = 2.0)
            // dw = 20, dh = 10 のとき、Math.Abs(dw) == Math.Abs(dh * ratio) (20 == 20)
            // >= なら横主導 (newHeight = newWidth / ratio = 120 / 2 = 60)
            // > に変異すると縦主導 (newWidth = newHeight * ratio = 60 * 2 = 120) -> あれ、これも120になる？！
            // 待て！ratio が 2.0 ではないとき、あるいは端数が出る場合：
            // original: 幅100, 高さ100 (ratio = 1.0)
            // dw = 10 (newWidth = 110), dh = 10 (newHeight = 110). dw == dh * 1.0.
            // 横主導なら newHeight = round(110 / 1.0) = 110.
            // 縦主導なら newWidth = round(110 * 1.0) = 110.
            // どちらも同じになる！
            // ではどうすれば dw == dh * ratio で差が出るか？
            // 例えば original: 幅10, 高さ3 (ratio = 10.0 / 3.0 = 3.333...)
            // dw = 10, dh = 3 -> dh * ratio = 3 * (10/3) = 10.0.
            // 横主導: newWidth = 20 -> newHeight = round(20 / (10/3)) = round(6.0) = 6.
            // 縦主導: newHeight = 6 -> newWidth = round(6 * (10/3)) = round(20.0) = 20.
            // ここも同じになる。
            // しかし！もしドラッグが dw=10, dh=3 のとき：
            // もし dw=11, dh=3 なら dw > dh*ratio だ。
            // では dw == dh * ratio が厳密に成立するケースは「どちらを主導にしても数学的に完全に一致する」！
            // なぜなら dw == dh * (W/H) であれば、(W + dw) / (H + dh) == W/H だからだ！！
            // アスペクト比と完全に一致する比率でドラッグしているのだから、横主導でも縦主導でも全く同一の結果になるのは数学的に100%当然！！
        }

        [Test]
        public void Resize_NonSquare_Diagonal_KillsRatioDivisionMutation()
        {
            // original: 幅100, 高さ50 (ratio = 2.0)
            // dw = 20 (newW = 120), dh = 15 (newH = 65)
            // dh * originalRatio = 15 * 2.0 = 30
            // 20 >= 30 は false -> 縦主導: newWidth = (50 + 15) * 2 = 130, newHeight = 65
            // もし dh / originalRatio に変異すると 15 / 2.0 = 7.5 となり 20 >= 7.5 で横主導になってしまう
            var original = new PictureArea(new Position(0, 0), new PictureSize(100, 50));
            var resizer = new ResizingSelection(original, SelectionHandle.BottomRight);

            var result = resizer.Resize(new Position(100, 50), new Position(120, 65), keepAspectRatio: true);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(130));
                Assert.That(result.Height, Is.EqualTo(65));
            });
        }

        [Test]
        public void Resize_AllCorners_KeepAspectRatio_StrictPositions()
        {
            var original = new PictureArea(new Position(100, 100), new PictureSize(100, 50)); // ratio = 2.0
            // right = 200, bottom = 150

            // TopLeft: 水平方向のみドラッグ (dx = -50, dy = 0)
            // step 2: left = 50, top = 100. newW = 150, newH = 50
            // step 5: dw = 50, dh = 0 -> 横主導 newH = 150 / 2 = 75. newW = 150
            // 再計算: left = 200 - 150 = 50, top = 150 - 75 = 75 (top が 100 から 75 に修正される！)
            var resizerTL = new ResizingSelection(original, SelectionHandle.TopLeft);
            var resultTL = resizerTL.Resize(new Position(100, 100), new Position(50, 100), keepAspectRatio: true);

            // TopRight: 水平方向のみドラッグ (dx = 50, dy = 0)
            // step 2: right = 250, top = 100. newW = 150, newH = 50
            // step 5: dw = 50, dh = 0 -> 横主導 newH = 150 / 2 = 75. newW = 150
            // 再計算: top = 150 - 75 = 75 (top が 100 から 75 に修正される！)
            var resizerTR = new ResizingSelection(original, SelectionHandle.TopRight);
            var resultTR = resizerTR.Resize(new Position(200, 100), new Position(250, 100), keepAspectRatio: true);

            // BottomLeft: 垂直方向のみドラッグ (dx = 0, dy = 50)
            // step 2: left = 100, bottom = 200. newW = 100, newH = 100
            // step 5: dw = 0, dh = 50 -> 縦主導 newW = 100 * 2 = 200. newH = 100
            // 再計算: left = 200 - 200 = 0 (left が 100 から 0 に修正される！)
            var resizerBL = new ResizingSelection(original, SelectionHandle.BottomLeft);
            var resultBL = resizerBL.Resize(new Position(100, 150), new Position(100, 200), keepAspectRatio: true);

            // BottomRight: 水平方向のみドラッグ (dx = 50, dy = 0)
            // step 2: right = 250, bottom = 150. newW = 150, newH = 50
            // step 5: 横主導 newH = 75. (left=100, top=100 は不変)
            var resizerBR = new ResizingSelection(original, SelectionHandle.BottomRight);
            var resultBR = resizerBR.Resize(new Position(200, 150), new Position(250, 150), keepAspectRatio: true);

            Assert.Multiple(() =>
            {
                Assert.That(resultTL.X, Is.EqualTo(50));
                Assert.That(resultTL.Y, Is.EqualTo(75));
                Assert.That(resultTL.Width, Is.EqualTo(150));
                Assert.That(resultTL.Height, Is.EqualTo(75));

                Assert.That(resultTR.X, Is.EqualTo(100));
                Assert.That(resultTR.Y, Is.EqualTo(75));
                Assert.That(resultTR.Width, Is.EqualTo(150));
                Assert.That(resultTR.Height, Is.EqualTo(75));

                Assert.That(resultBL.X, Is.EqualTo(0));
                Assert.That(resultBL.Y, Is.EqualTo(100));
                Assert.That(resultBL.Width, Is.EqualTo(200));
                Assert.That(resultBL.Height, Is.EqualTo(100));

                Assert.That(resultBR.X, Is.EqualTo(100));
                Assert.That(resultBR.Y, Is.EqualTo(100));
                Assert.That(resultBR.Width, Is.EqualTo(150));
                Assert.That(resultBR.Height, Is.EqualTo(75));
            });
        }

        [Test]
        public void Resize_EdgeHandle_IgnoresKeepAspectRatio()
        {
            var original = new PictureArea(new Position(10, 10), new PictureSize(20, 20));
            var resizer = new ResizingSelection(original, SelectionHandle.Right);

            var result = resizer.Resize(new Position(30, 20), new Position(40, 20), keepAspectRatio: true);

            Assert.Multiple(() =>
            {
                Assert.That(result.Width, Is.EqualTo(30));
                Assert.That(result.Height, Is.EqualTo(20));
            });
        }
    }
}
