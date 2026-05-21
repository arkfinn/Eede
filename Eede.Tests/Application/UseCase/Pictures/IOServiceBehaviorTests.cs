using Eede.Application.UseCase.Pictures;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Eede.Application.Tests.UseCase.Pictures
{
    [TestFixture]
    public class IOServiceBehaviorTests
    {
        private Mock<IPictureRepository> _repositoryMock;
        private Mock<Eede.Application.Infrastructure.ISettingsRepository> _settingsRepositoryMock;
        private FilePath _testPath;
        private Picture _testPicture;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IPictureRepository>();
            _settingsRepositoryMock = new Mock<Eede.Application.Infrastructure.ISettingsRepository>();
            _settingsRepositoryMock.Setup(r => r.LoadAsync()).ReturnsAsync(new Eede.Application.Settings.AppSettings());
            _testPath = new FilePath("test.png");
            _testPicture = Picture.CreateEmpty(new PictureSize(1, 1));
        }

        [Test]
        public async Task LoadPictureUseCase_ShouldCallRepository()
        {
            _repositoryMock.Setup(r => r.LoadAsync(_testPath)).ReturnsAsync(_testPicture);
            var useCase = new LoadPictureUseCase(_repositoryMock.Object, _settingsRepositoryMock.Object);

            var result = await useCase.ExecuteAsync(_testPath);

            Assert.That(result, Is.EqualTo(_testPicture));
            _repositoryMock.Verify(r => r.LoadAsync(_testPath), Times.Once);
        }

        [Test]
        public async Task SavePictureUseCase_ShouldCallRepository()
        {
            var useCase = new SavePictureUseCase(_repositoryMock.Object, _settingsRepositoryMock.Object);

            await useCase.ExecuteAsync(_testPicture, _testPath);

            _repositoryMock.Verify(r => r.SaveAsync(_testPicture, _testPath), Times.Once);
        }
    }
}
