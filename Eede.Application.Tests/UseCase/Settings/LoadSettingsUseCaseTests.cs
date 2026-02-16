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
}
