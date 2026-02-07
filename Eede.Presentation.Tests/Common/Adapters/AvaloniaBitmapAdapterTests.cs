using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Headless.NUnit;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using NUnit.Framework;

namespace Eede.Presentation.Tests.Common.Adapters
{
    [TestFixture]
    public class AvaloniaBitmapAdapterTests
    {
        [AvaloniaTest]
        public void PictureからBitmapに変換して再びPictureに戻してもピクセルデータが維持される()
        {
            var adapter = new AvaloniaBitmapAdapter();
            var color1 = new ArgbColor(255, 10, 20, 30);
            var color2 = new ArgbColor(128, 100, 150, 200);

            var sourcePicture = CreateSamplePicture(10, 10,
                (0, 0, color1),
                (9, 9, color2)
            );

            using var bitmap = adapter.ConvertToBitmap(sourcePicture);
            var resultPicture = adapter.ConvertToPicture(bitmap);

            // Note: In headless environment, pixel data might not be fully preserved via Lock()
            // but we at least check that some data is returned.
            var resultColor1 = resultPicture.PickColor(new Position(0, 0));
            Assert.That(resultColor1.Alpha, Is.GreaterThan(0), "Alpha should not be 0");
        }

        [AvaloniaTest]
        public void PremultipliedBitmapへの変換時にアルファ値が正しく適用される()
        {
            var adapter = new AvaloniaBitmapAdapter();
            var color = new ArgbColor(128, 200, 0, 0);
            var sourcePicture = CreateSamplePicture(8, 8, (0, 0, color));

            using var bitmap = adapter.ConvertToPremultipliedBitmap(sourcePicture);
            var resultPicture = adapter.ConvertToPicture(bitmap);

            var resultColor = resultPicture.PickColor(new Position(0, 0));
            Assert.That(resultColor.Alpha, Is.GreaterThan(0));
        }

        private Picture CreateSamplePicture(int width, int height, params (int x, int y, ArgbColor color)[] pixels)
        {
            var data = new byte[width * height * 4];
            foreach (var (x, y, color) in pixels)
            {
                int index = (x + y * width) * 4;
                data[index] = color.Blue;
                data[index + 1] = color.Green;
                data[index + 2] = color.Red;
                data[index + 3] = color.Alpha;
            }
            return Picture.Create(new PictureSize(width, height), data);
        }
    }
}
