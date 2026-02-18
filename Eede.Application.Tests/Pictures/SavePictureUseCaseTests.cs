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
    public class SavePictureUseCaseTests
    {
        [Test]
        public async Task ExecuteAsync_ShouldCallRepositorySaveAsync()
        {
            var mockRepo = new Mock<IPictureRepository>();
            var mockSettingsRepo = new Mock<ISettingsRepository>();
            mockSettingsRepo.Setup(r => r.LoadAsync()).ReturnsAsync(new AppSettings());

            var useCase = new SavePictureUseCase(mockRepo.Object, mockSettingsRepo.Object);
            var picture = Picture.CreateEmpty(new PictureSize(32, 32));
            var path = new FilePath("test.png");

            await useCase.ExecuteAsync(picture, path);

            mockRepo.Verify(r => r.SaveAsync(picture, path), Times.Once);
            mockSettingsRepo.Verify(r => r.SaveAsync(It.Is<AppSettings>(s => s.RecentFiles[0].Path == "test.png")), Times.Once);
        }
    }
}
