using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools
{
    [TestFixture]
    public class FillTests
    {
        [Test]
        public void Fill_ShouldCompleteWithoutHanging()
        {
            // Arrange: 4x4の白い画像を作成
            var size = new PictureSize(4, 4);
            var white = new ArgbColor(255, 255, 255, 255);
            var picture = CreateMonoColorPicture(size, white);

            // Act: 赤色で(0, 0)から塗りつぶす
            var red = new ArgbColor(255, 255, 0, 0);
            var drawer = new Drawer(picture, new PenStyle(new DirectImageBlender(), red, 1));
            var result = drawer.Fill(new Position(0, 0));

            // Assert: 全てのピクセルが赤くなっていることを確認（ハングアップせずにここに到達すること）
            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    Assert.That(result.PickColor(new Position(x, y)), Is.EqualTo(red), $"Pixel at ({x}, {y}) should be red.");
                }
            }
        }

        [Test]
        public void Fill_ShouldOnlyFillTargetArea()
        {
            // Arrange: 左半分が白、右半分が黒の4x4画像を作成
            var size = new PictureSize(4, 4);
            var white = new ArgbColor(255, 255, 255, 255);
            var black = new ArgbColor(255, 0, 0, 0);
            var data = new byte[4 * 4 * 4];
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    int i = (x * 4) + (y * 16);
                    var color = x < 2 ? white : black;
                    data[i] = color.Blue;
                    data[i + 1] = color.Green;
                    data[i + 2] = color.Red;
                    data[i + 3] = color.Alpha;
                }
            }
            var picture = Picture.Create(size, data);

            // Act: 左上(0, 0)の白色エリアを赤色で塗りつぶす
            var red = new ArgbColor(255, 255, 0, 0);
            var drawer = new Drawer(picture, new PenStyle(new DirectImageBlender(), red, 1));
            var result = drawer.Fill(new Position(0, 0));

            // Assert: 左半分(x < 2)は赤、右半分(x >= 2)は黒のままであること
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    var expected = x < 2 ? red : black;
                    Assert.That(result.PickColor(new Position(x, y)), Is.EqualTo(expected), $"Pixel at ({x}, {y}) failed.");
                }
            }
        }

        [Test]
        public void Fill_WhenSourceAndTargetAreSameColor_ShouldReturnOriginal()
        {
            // Arrange: 全面赤の画像
            var size = new PictureSize(2, 2);
            var red = new ArgbColor(255, 255, 0, 0);
            var picture = CreateMonoColorPicture(size, red);

            // Act: 赤色で塗りつぶしを試みる
            var drawer = new Drawer(picture, new PenStyle(new DirectImageBlender(), red, 1));
            var result = drawer.Fill(new Position(0, 0));

            // Assert: 中身が同じであることを確認
            Assert.That(result.CloneImage(), Is.EqualTo(picture.CloneImage()));
        }

        private Picture CreateMonoColorPicture(PictureSize size, ArgbColor color)
        {
            var data = new byte[size.Width * size.Height * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = color.Blue;
                data[i + 1] = color.Green;
                data[i + 2] = color.Red;
                data[i + 3] = color.Alpha;
            }
            return Picture.Create(size, data);
        }
    }
}
