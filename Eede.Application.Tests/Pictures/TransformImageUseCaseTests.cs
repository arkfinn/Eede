using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System.Linq;

namespace Eede.Application.Tests.Pictures
{
    [TestFixture]
    public class TransformImageUseCaseTests
    {
        private TransformImageUseCase _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new TransformImageUseCase();
        }

        [Test]
        public void Execute_WholePicture_FlipHorizontally_ReturnsFlippedPicture()
        {
            // Arrange
            var size = new PictureSize(2, 1);
            var data = new byte[] 
            { 
                1, 1, 1, 1,
                2, 2, 2, 2
            };
            var source = Picture.Create(size, data);
            var action = PictureActions.FlipHorizontal;

            // Act
            var result = _sut.Execute(source, action);

            // Assert
            var resultSpan = result.AsSpan();
            Assert.That(resultSpan[0], Is.EqualTo(2)); 
            Assert.That(resultSpan[4], Is.EqualTo(1));
        }

        [Test]
        public void Execute_SelectedRegion_FlipHorizontally_ReturnsPartiallyFlippedPicture()
        {
            // Arrange
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
            var result = _sut.Execute(source, action, area);

            // Assert
            var resultSpan = result.AsSpan();
            Assert.That(resultSpan[0], Is.EqualTo(1));
            Assert.That(resultSpan[4], Is.EqualTo(3));
            Assert.That(resultSpan[8], Is.EqualTo(2));
            Assert.That(resultSpan[12], Is.EqualTo(4));
        }
    }
}
