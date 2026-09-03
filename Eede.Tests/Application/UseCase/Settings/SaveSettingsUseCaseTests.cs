using NUnit.Framework;
using Moq;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Application.UseCase.Settings;
using System.Threading.Tasks;

namespace Eede.Application.Tests.UseCase.Settings;

[TestFixture]
public class SaveSettingsUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_WhenRepositorySaveReturnsTrue_ShouldReturnTrue()
    {
        var repositoryMock = new Mock<ISettingsRepository>();
        var settings = new AppSettings { GridWidth = 48, GridHeight = 64 };
        repositoryMock.Setup(x => x.SaveAsync(settings)).ReturnsAsync(true);

        var useCase = new SaveSettingsUseCase(repositoryMock.Object);
        var result = await useCase.ExecuteAsync(settings);

        Assert.That(result, Is.True);
        repositoryMock.Verify(x => x.SaveAsync(settings), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WhenRepositorySaveReturnsFalse_ShouldReturnFalse()
    {
        var repositoryMock = new Mock<ISettingsRepository>();
        var settings = new AppSettings { GridWidth = 48, GridHeight = 64 };
        repositoryMock.Setup(x => x.SaveAsync(settings)).ReturnsAsync(false);

        var useCase = new SaveSettingsUseCase(repositoryMock.Object);
        var result = await useCase.ExecuteAsync(settings);

        Assert.That(result, Is.False);
        repositoryMock.Verify(x => x.SaveAsync(settings), Times.Once);
    }

    [Test]
    public void ExecuteAsync_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        var repositoryMock = new Mock<ISettingsRepository>();
        var settings = new AppSettings { GridWidth = 48, GridHeight = 64 };
        repositoryMock.Setup(x => x.SaveAsync(settings)).ThrowsAsync(new System.IO.IOException("File write error"));

        var useCase = new SaveSettingsUseCase(repositoryMock.Object);

        Assert.ThrowsAsync<System.IO.IOException>(async () => await useCase.ExecuteAsync(settings));
        repositoryMock.Verify(x => x.SaveAsync(settings), Times.Once);
    }
}
