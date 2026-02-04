using Eede.Application.PaintLayers;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System.Linq;

namespace Eede.Application.Tests.PaintLayers
{
    [TestFixture]
    public class CursorLayerTests
    {
        private class DirectImageTransfer : IImageTransfer
        {
            public Picture Transfer(Picture source, Magnification magnification)
            {
                return source;
            }
        }

        private Picture CreateFilledPicture(PictureSize size, ArgbColor color)
        {
            byte[] data = new byte[size.Width * size.Height * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = color.Blue;
                data[i + 1] = color.Green;
                data[i + 2] = color.Red;
                data[i + 3] = color.Alpha;
            }
            return Picture.Create(size, data);
        }

        [Test]
        public void Painted_Should_Not_Darken_SemiTransparent_Background()
        {
            // 1. 背景を半透明（Alpha=128）の赤にする
            var size = new PictureSize(10, 10);
            var semiTransparentRed = new ArgbColor(128, 255, 0, 0);
            var background = CreateFilledPicture(size, semiTransparentRed);

            // 2. ペンを設定 (ここでは無関係な色だが、ブレンドモードが重要)
            // Application層のテストなので、Blenderの実装詳細はDomainに依存するが、
            // ここではEede.Domain.ImageEditing.Blending.AlphaImageBlenderを使用する想定。
            // しかしテスト内ではBlenderをMockするか、実物を使うか。
            // CursorLayerはPenStyleを受け取る。
            var penColor = new ArgbColor(255, 0, 0, 255); // Blue
            var blender = new Eede.Domain.ImageEditing.Blending.AlphaImageBlender();
            var penStyle = new PenStyle(blender, penColor, 1);

            // 3. CursorLayer作成
            var paintSize = new MagnifiedSize(size, new Magnification(1));
            var transfer = new DirectImageTransfer();
            var cursorPosition = new Position(5, 5); // 真ん中にカーソル
            var layer = new CursorLayer(paintSize, background, penStyle, cursorPosition, transfer);

            // 4. 描画実行
            var result = layer.Painted(background); // destinationは無視される実装だが渡す

            // 5. 検証
            // カーソル位置 (5,5) は青になるはず (AlphaBlend: 128赤 + 255青 = ほぼ青)
            // カーソル以外の位置 (0,0) は「半透明赤 (128)」のままであるべき。
            // もし「背景」が二重にブレンドされていたら、128 + 128 でもっと濃くなっているはず。
            
            var pixel = result.PickColor(new Position(0, 0));
            Assert.That(pixel.Alpha, Is.EqualTo(128), "CursorLayer should not darken the background pixels where cursor is not present.");
        }

        [Test]
        public void Painted_With_DirectBlender_Should_Not_Erase_Background()
        {
            var size = new PictureSize(10, 10);
            var background = CreateFilledPicture(size, new ArgbColor(255, 255, 0, 0)); // Red

            var penColor = new ArgbColor(255, 0, 0, 255); // Blue
            var blender = new Eede.Domain.ImageEditing.Blending.DirectImageBlender();
            var penStyle = new PenStyle(blender, penColor, 1);

            var paintSize = new MagnifiedSize(size, new Magnification(1));
            var transfer = new DirectImageTransfer();
            var cursorPosition = new Position(5, 5);
            var layer = new CursorLayer(paintSize, background, penStyle, cursorPosition, transfer);

            var result = layer.Painted(background);

            Assert.That(result.PickColor(cursorPosition).Blue, Is.EqualTo(255));
            
            var bgPixel = result.PickColor(new Position(0, 0));
            Assert.That(bgPixel.Red, Is.EqualTo(255), "Background should remain red.");
            Assert.That(bgPixel.Alpha, Is.EqualTo(255), "Background should remain opaque.");
        }
    }
}
