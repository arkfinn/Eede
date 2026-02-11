using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Services;
using Moq;
using NUnit.Framework;

namespace Eede.Presentation.Tests
{
    [TestFixture]
    public class RegressionTests_SelectionPreview
    {
        [Test]
        public void Painted_ShouldIncludePreviewAndClearOriginalArea()
        {
            // 背景を青(B=255)で塗りつぶした10x10の画像を作成
            var bgPixels = new byte[10 * 10 * 4];
            for (int i = 0; i < bgPixels.Length; i += 4)
            {
                bgPixels[i] = 255; // B
                bgPixels[i + 3] = 255; // A
            }
            var bgPicture = Picture.Create(new PictureSize(10, 10), bgPixels);
            var buffer = new DrawingBuffer(bgPicture);

            // プレビュー情報: (0,0)から2x2を切り出し、(5,5)へ移動させている状態
            var previewPixels = new byte[2 * 2 * 4];
            for (int i = 0; i < previewPixels.Length; i += 4)
            {
                previewPixels[i + 2] = 255; // R
                previewPixels[i + 3] = 255; // A
            }
            var previewPicture = Picture.Create(new PictureSize(2, 2), previewPixels);
            var info = new SelectionPreviewInfo(
                previewPicture,
                new Position(5, 5),
                SelectionPreviewType.CutAndMove,
                new PictureArea(new Position(0, 0), new PictureSize(2, 2)));

            var sessionProvider = new DrawingSessionProvider();
            sessionProvider.Update(new DrawingSession(bgPicture).UpdatePreviewContent(info));

            var coordinator = new InteractionCoordinator(sessionProvider);
            coordinator.SyncWithSession();

            // Paintedを実行（IdentityImageTransferでトーン変換なし）
            var result = coordinator.Painted(
                buffer,
                new PenStyle(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1),
                new IdentityImageTransfer());

            // 検証:
            // 1. 元の領域 (0,0) がクリアされている（Alpha = 0）
            var color00 = result.PickColor(new Position(0, 0));
            Assert.That(color00.Alpha, Is.EqualTo(0), "Original area (0,0) should be cleared");

            // 2. 移動先 (5,5) にプレビューが合成されている（Red = 255）
            var color55 = result.PickColor(new Position(5, 5));
            Assert.That(color55.Red, Is.EqualTo(255), "Preview should be blended at (5,5)");

            // 3. 関係ない領域 (9,9) は背景のまま（Blue = 255）
            var color99 = result.PickColor(new Position(9, 9));
            Assert.That(color99.Blue, Is.EqualTo(255), "Background should remain at (9,9)");
        }
    }
}
