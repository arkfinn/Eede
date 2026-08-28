using Eede.Application.Pictures;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Moq;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace Eede.Application.Tests.Pictures
{
    public class PictureIOServiceTests
    {
        private Mock<ISavePictureUseCase> _mockSaveUseCase;
        private Mock<ILoadPictureUseCase> _mockLoadUseCase;
        private PictureIOService _service;

        [SetUp]
        public void SetUp()
        {
            _mockSaveUseCase = new Mock<ISavePictureUseCase>();
            _mockLoadUseCase = new Mock<ILoadPictureUseCase>();
            _service = new PictureIOService(_mockSaveUseCase.Object, _mockLoadUseCase.Object);
        }

        [Test]
        public async Task SaveAsync_ShouldCallSaveUseCase()
        {
            var picture = Picture.CreateEmpty(new PictureSize(16, 16));
            var path = new FilePath("test.png");

            await _service.SaveAsync(picture, path);

            _mockSaveUseCase.Verify(x => x.ExecuteAsync(picture, path), Times.Once);
        }

        [Test]
        public async Task LoadAsync_ShouldCallLoadUseCaseAndReturnPicture()
        {
            var path = new FilePath("test.png");
            var expectedPicture = Picture.CreateEmpty(new PictureSize(16, 16));
            _mockLoadUseCase.Setup(x => x.ExecuteAsync(path)).ReturnsAsync(expectedPicture);

            var result = await _service.LoadAsync(path);

            _mockLoadUseCase.Verify(x => x.ExecuteAsync(path), Times.Once);
            Assert.That(result, Is.EqualTo(expectedPicture));
        }

        [Test]
        public void SaveAsync_WhenUseCaseThrows_ShouldPropagateException()
        {
            var picture = Picture.CreateEmpty(new PictureSize(16, 16));
            var path = new FilePath("test.png");
            _mockSaveUseCase.Setup(x => x.ExecuteAsync(picture, path)).ThrowsAsync(new IOException("Test exception"));

            Assert.ThrowsAsync<IOException>(async () => await _service.SaveAsync(picture, path));
        }

        [Test]
        public void LoadAsync_WhenUseCaseThrows_ShouldPropagateException()
        {
            var path = new FilePath("test.png");
            _mockLoadUseCase.Setup(x => x.ExecuteAsync(path)).ThrowsAsync(new IOException("Test exception"));

            Assert.ThrowsAsync<IOException>(async () => await _service.LoadAsync(path));
        }
    }
}
