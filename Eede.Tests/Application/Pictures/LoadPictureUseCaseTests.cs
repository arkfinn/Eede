using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Eede.Application.Tests.Pictures
{
    public class LoadPictureUseCaseTests
    {
        [Test]
        public async Task ExecuteAsync_ShouldCallRepositoryLoadAsync()
        {
            var mockRepo = new Mock<IPictureRepository>();
            var expectedPicture = Picture.CreateEmpty(new PictureSize(32, 32));
            var path = new FilePath("test.png");
            mockRepo.Setup(r => r.LoadAsync(path)).ReturnsAsync(expectedPicture);

            var mockSettingsRepo = new Mock<ISettingsRepository>();
            mockSettingsRepo.Setup(r => r.LoadAsync()).ReturnsAsync(new AppSettings());

            var useCase = new LoadPictureUseCase(mockRepo.Object, mockSettingsRepo.Object);
            var result = await useCase.ExecuteAsync(path);

            Assert.That(result, Is.EqualTo(expectedPicture));
            mockRepo.Verify(r => r.LoadAsync(path), Times.Once);
            mockSettingsRepo.Verify(r => r.SaveAsync(It.Is<AppSettings>(s => s.RecentFiles[0].Path == "test.png")), Times.Once);
        }
    }
}
