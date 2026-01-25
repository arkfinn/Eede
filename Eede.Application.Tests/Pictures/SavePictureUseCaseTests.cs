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
            var useCase = new SavePictureUseCase(mockRepo.Object);
            var picture = Picture.CreateEmpty(new PictureSize(32, 32));
            var path = new FilePath("test.png");

            await useCase.ExecuteAsync(picture, path);

            mockRepo.Verify(r => r.SaveAsync(picture, path), Times.Once);
        }
    }
}
