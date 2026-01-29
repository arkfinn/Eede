using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System.Linq;

namespace Eede.Application.Tests.Pictures
{
    [TestFixture]
    public class PictureEditingUseCaseTests
    {
        private PictureEditingUseCase _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new PictureEditingUseCase();
        }

        private Picture CreateTestPicture(int width, int height, byte fillValue)
        {
            var data = Enumerable.Repeat(fillValue, width * height * 4).ToArray();
            return Picture.Create(new PictureSize(width, height), data);
        }

        [Test]
        public void ExecuteAction_WholePicture_FlipHorizontally_ReturnsFlippedPicture()
        {
            // Arrange
            // 2x1 picture: [Left=1, Right=2]
            var size = new PictureSize(2, 1);
            var data = new byte[] 
            { 
                1, 1, 1, 1, // Left pixel (RGBA)
                2, 2, 2, 2  // Right pixel (RGBA)
            };
            var source = Picture.Create(size, data);
            var action = PictureActions.FlipHorizontal;

            // Act
            var result = _sut.ExecuteAction(source, action);

            // Assert
            Assert.That(result.Previous, Is.EqualTo(source));
            Assert.That(result.SelectingArea, Is.Null);
            
            // Expected: [Left=2, Right=1]
            var resultSpan = result.Updated.AsSpan();
            Assert.That(resultSpan[0], Is.EqualTo(2)); 
            Assert.That(resultSpan[4], Is.EqualTo(1));
        }

        [Test]
        public void ExecuteAction_SelectedRegion_FlipHorizontally_ReturnsPartiallyFlippedPicture()
        {
            // Arrange
            // 4x1 picture: [1, 2, 3, 4]
            // Select middle 2: [2, 3] -> Flip -> [3, 2]
            // Expected: [1, 3, 2, 4]
            var size = new PictureSize(4, 1);
            var data = new byte[] 
            {
                1,1,1,1,
                2,2,2,2, 
                3,3,3,3, 
                4,4,4,4 
            };
            var source = Picture.Create(size, data);
            var action = PictureActions.FlipHorizontal;
            var area = new PictureArea(new Position(1, 0), new PictureSize(2, 1));

            // Act
            var result = _sut.ExecuteAction(source, action, area);

            // Assert
            Assert.That(result.Previous, Is.EqualTo(source));
            Assert.That(result.SelectingArea, Is.EqualTo(area));

            var resultSpan = result.Updated.AsSpan();
            Assert.That(resultSpan[0], Is.EqualTo(1)); // Unchanged
            Assert.That(resultSpan[4], Is.EqualTo(3)); // Flipped
            Assert.That(resultSpan[8], Is.EqualTo(2)); // Flipped
            Assert.That(resultSpan[12], Is.EqualTo(4)); // Unchanged
        }

        [Test]
        public void PushToCanvas_ExtractsRegionFromSource()
        {
            // Arrange
            var source = CreateTestPicture(10, 10, 1);
            
            var fromSize = new PictureSize(4, 4);
            var fromData = Enumerable.Repeat((byte)10, 4 * 4 * 4).ToArray();
            var fromPicture = Picture.Create(fromSize, fromData);

            var rect = new PictureArea(new Position(1, 1), new PictureSize(2, 2));

            // Act
            var result = _sut.PushToCanvas(source, fromPicture, rect);

            // Assert
            Assert.That(result.Previous, Is.EqualTo(source));
            Assert.That(result.SelectingArea, Is.EqualTo(rect));
            Assert.That(result.Updated.Width, Is.EqualTo(2));
            Assert.That(result.Updated.Height, Is.EqualTo(2));
            
            var resultSpan = result.Updated.AsSpan();
            Assert.That(resultSpan[0], Is.EqualTo(10));
        }

        [Test]
        public void PullFromCanvas_BlendsSourceIntoTarget()
        {
            // Arrange
            // Target: 4x4, filled with 0
            var target = CreateTestPicture(4, 4, 0);
            
            // Source: 2x2, filled with 255
            var source = CreateTestPicture(2, 2, 255);
            
            var position = new Position(1, 1);
            var blender = new DirectImageBlender();

            // Act
            var result = _sut.PullFromCanvas(target, source, position, blender);

            // Assert
            Assert.That(result.Previous, Is.EqualTo(target));
            Assert.That(result.SelectingArea, Is.Null);
            
            // Verify center pixels are 255
            // (1,1) -> index: (1 + 1*4)*4 = 20
            var resultSpan = result.Updated.AsSpan();
            Assert.That(resultSpan[20], Is.EqualTo(255));
            
            // Verify corner is 0
            Assert.That(resultSpan[0], Is.EqualTo(0));
        }
    }
}
