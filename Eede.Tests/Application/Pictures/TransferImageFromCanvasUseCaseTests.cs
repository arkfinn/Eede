using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System.Linq;

namespace Eede.Application.Tests.Pictures
{
    [TestFixture]
    public class TransferImageFromCanvasUseCaseTests
    {
        private TransferImageFromCanvasUseCase _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new TransferImageFromCanvasUseCase();
        }

        private Picture CreateTestPicture(int width, int height, byte fillValue)
        {
            var data = Enumerable.Repeat(fillValue, width * height * 4).ToArray();
            return Picture.Create(new PictureSize(width, height), data);
        }

        [Test]
        public void Execute_BlendsSourceIntoTarget()
        {
            // Arrange
            var target = CreateTestPicture(4, 4, 0);
            var source = CreateTestPicture(2, 2, 255);
            var position = new Position(1, 1);
            var blender = new DirectImageBlender();

            // Act
            var result = _sut.Execute(target, source, position, blender);

            // Assert
            var resultSpan = result.AsSpan();
            // (1,1) -> index: (1 + 1*4)*4 = 20
            Assert.That(resultSpan[20], Is.EqualTo(255));
            Assert.That(resultSpan[0], Is.EqualTo(0));
        }
    }
}
