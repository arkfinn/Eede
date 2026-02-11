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

            // 2. ペンを設定
            var penColor = new ArgbColor(255, 0, 0, 255); // Blue
            var blender = new Eede.Domain.ImageEditing.Blending.AlphaImageBlender();
            var penStyle = new PenStyle(blender, penColor, 1);

            // 3. CursorLayer作成 (transferは何もしないもの)
            var paintSize = new MagnifiedSize(size, new Magnification(1));
            var transfer = new DirectImageTransfer();
            var cursorPosition = new Position(5, 5);
            var layer = new CursorLayer(paintSize, background, penStyle, cursorPosition, transfer);

            // 4. 描画実行
            // 修正：以前の実装との互換性のために背景を渡していたが、
            // 新しい実装では引数がベースになるため、空の画像を渡すとカーソル部分だけが確認できる。
            // しかし、本来の使われ方は「背景画像に対して実行する」ことなので、
            // 「背景画像に対して、カーソルがない場所の色が変わらないこと」を確認する。
            var result = layer.Painted(background);

            // 5. 検証
            // カーソル以外の位置 (0,0) は「半透明赤 (128)」のままであるべき。
            // ※新しい CursorLayer.Painted は destination に対して描画するため、
            // テスト内で引数に background を渡すと、128赤の上に何も重ならない場所は 128赤のままになるはず。

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

        [Test]
        public void Painted_Should_Apply_Alpha_Tone_To_Cursor()
        {
            // 1. 赤(RGB)でアルファが128のペンを設定
            var size = new PictureSize(1, 1);
            var penColor = new ArgbColor(128, 255, 0, 0);
            var blender = new Eede.Domain.ImageEditing.Blending.DirectImageBlender();
            var penStyle = new PenStyle(blender, penColor, 1);

            // 2. 空の背景
            var background = CreateFilledPicture(size, new ArgbColor(0, 0, 0, 0));

            // 3. アルファトーン変換を使用
            var transfer = new Eede.Domain.ImageEditing.Transformation.AlphaToneImageTransfer();
            var paintSize = new MagnifiedSize(size, new Magnification(1));
            var layer = new CursorLayer(paintSize, background, penStyle, new Position(0, 0), transfer);

            // 4. 実行
            var result = layer.Painted(background);

            // 5. 検証
            var pixel = result.PickColor(new Position(0, 0));
            // アルファモードなら、RGBが元のAlpha(128)になっているべき
            Assert.That(pixel.Red, Is.EqualTo(128), "Red should be 128 (grayscale of original alpha).");
            Assert.That(pixel.Green, Is.EqualTo(128));
            Assert.That(pixel.Blue, Is.EqualTo(128));
        }

        [Test]
        public void Painted_Should_Apply_RGB_Tone_To_Cursor()
        {
            // 1. 赤(RGB)でアルファが128のペンを設定
            var size = new PictureSize(1, 1);
            var penColor = new ArgbColor(128, 255, 0, 0);
            var blender = new Eede.Domain.ImageEditing.Blending.DirectImageBlender();
            var penStyle = new PenStyle(blender, penColor, 1);

            // 2. 空の背景
            var background = CreateFilledPicture(size, new ArgbColor(0, 0, 0, 0));

            // 3. RGBトーン変換を使用
            var transfer = new Eede.Domain.ImageEditing.Transformation.RGBToneImageTransfer();
            var paintSize = new MagnifiedSize(size, new Magnification(1));
            var layer = new CursorLayer(paintSize, background, penStyle, new Position(0, 0), transfer);

            // 4. 実行
            var result = layer.Painted(background);

            // 5. 検証
            var pixel = result.PickColor(new Position(0, 0));
            // RGBモードなら、元のRGB(255,0,0)が維持され、Alphaは255になっているべき
            Assert.That(pixel.Red, Is.EqualTo(255), "Red should be 255.");
            Assert.That(pixel.Green, Is.EqualTo(0));
            Assert.That(pixel.Blue, Is.EqualTo(0));
            Assert.That(pixel.Alpha, Is.EqualTo(255));
        }
    }
}
