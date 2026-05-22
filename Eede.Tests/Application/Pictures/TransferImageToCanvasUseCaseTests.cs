using Eede.Application.UseCase.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System.Linq;

namespace Eede.Application.Tests.Pictures
{
    [TestFixture]
    public class TransferImageToCanvasUseCaseTests
    {
        private TransferImageToCanvasUseCase _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new TransferImageToCanvasUseCase();
        }

        [Test]
        public void Execute_ExtractsRegionFromSource()
        {
            // Arrange
            var size = new PictureSize(4, 4);
            var data = Enumerable.Repeat((byte)10, 4 * 4 * 4).ToArray();
            var source = Picture.Create(size, data);
            var rect = new PictureArea(new Position(1, 1), new PictureSize(2, 2));

            // Act
            var result = _sut.Execute(source, rect);

            // Assert
            Assert.That(result.Width, Is.EqualTo(2));
            Assert.That(result.Height, Is.EqualTo(2));
            var resultSpan = result.AsSpan();
            Assert.That(resultSpan[0], Is.EqualTo(10));
        }
    }
}
