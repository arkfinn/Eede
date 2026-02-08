using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using Eede.Domain.Palettes;

namespace Eede.Application.Tests.UseCase.Pictures
{
    [TestFixture]
    public class CanvasOperationBehaviorTests
    {
        [Test]
        public void TransferImageToCanvasBehaviorTest()
        {
            var useCase = new TransferImageToCanvasUseCase();
            var size = new PictureSize(10, 10);
            var color = new ArgbColor(255, 10, 20, 30);
            
            // (2, 2) に色がついた画像を作成
            var dot = Picture.Create(new PictureSize(1, 1), new byte[] { color.Blue, color.Green, color.Red, color.Alpha });
            var source = Picture.CreateEmpty(size).Blend(new DirectImageBlender(), 
                dot, 
                new Position(2, 2));

            // (2, 2) を含む 2x2 の範囲を切り出す
            var rect = new PictureArea(new Position(2, 2), new PictureSize(2, 2));
            var result = useCase.Execute(source, rect);

            Assert.That(result.Size.Width, Is.EqualTo(2));
            Assert.That(result.Size.Height, Is.EqualTo(2));
            Assert.That(result.PickColor(new Position(0, 0)), Is.EqualTo(color), "切り出した画像の左上が元の (2, 2) の色であること");
        }

        [Test]
        public void TransformImageBehaviorTest()
        {
            var useCase = new TransformImageUseCase();
            var size = new PictureSize(2, 1);
            var color1 = new ArgbColor(255, 255, 0, 0); // Red
            var color2 = new ArgbColor(255, 0, 255, 0); // Green
            
            // [Red, Green] という 2x1 の画像を作成
            var redDot = Picture.Create(new PictureSize(1, 1), new byte[] { color1.Blue, color1.Green, color1.Red, color1.Alpha });
            var greenDot = Picture.Create(new PictureSize(1, 1), new byte[] { color2.Blue, color2.Green, color2.Red, color2.Alpha });
            var source = Picture.CreateEmpty(size)
                .Blend(new DirectImageBlender(), redDot, new Position(0, 0))
                .Blend(new DirectImageBlender(), greenDot, new Position(1, 0));

            // 左右反転
            var result = useCase.Execute(source, PictureActions.FlipHorizontal);

            Assert.That(result.PickColor(new Position(0, 0)), Is.EqualTo(color2), "反転後の左端が Green であること");
            Assert.That(result.PickColor(new Position(1, 0)), Is.EqualTo(color1), "反転後の右端が Red であること");
        }
    }
}
