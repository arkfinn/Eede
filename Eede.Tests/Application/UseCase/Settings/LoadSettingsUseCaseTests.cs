using NUnit.Framework;
using Moq;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Application.UseCase.Settings;
using System.Threading.Tasks;

namespace Eede.Application.Tests.UseCase.Settings;

[TestFixture]
public class LoadSettingsUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldCallRepositoryLoad()
    {
        var repositoryMock = new Mock<ISettingsRepository>();
        var expected = new AppSettings { GridWidth = 16, GridHeight = 16 };
        repositoryMock.Setup(x => x.LoadAsync()).ReturnsAsync(expected);

        var useCase = new LoadSettingsUseCase(repositoryMock.Object);
        var actual = await useCase.ExecuteAsync();

        Assert.That(actual, Is.EqualTo(expected));
        repositoryMock.Verify(x => x.LoadAsync(), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WhenRepositoryReturnsNull_ShouldReturnNull()
    {
        var repositoryMock = new Mock<ISettingsRepository>();
        repositoryMock.Setup(x => x.LoadAsync()).ReturnsAsync((AppSettings?)null!);

        var useCase = new LoadSettingsUseCase(repositoryMock.Object);
        var actual = await useCase.ExecuteAsync();

        Assert.That(actual, Is.Null);
        repositoryMock.Verify(x => x.LoadAsync(), Times.Once);
    }

    [Test]
    public void ExecuteAsync_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        var repositoryMock = new Mock<ISettingsRepository>();
        repositoryMock.Setup(x => x.LoadAsync()).ThrowsAsync(new System.IO.IOException("File load error"));

        var useCase = new LoadSettingsUseCase(repositoryMock.Object);

        Assert.ThrowsAsync<System.IO.IOException>(async () => await useCase.ExecuteAsync());
        repositoryMock.Verify(x => x.LoadAsync(), Times.Once);
    }
}
