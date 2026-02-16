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
    public async Task ExecuteAsync_ShouldCallRepositorySave()
    {
        var repositoryMock = new Mock<ISettingsRepository>();
        var settings = new AppSettings { GridWidth = 48, GridHeight = 64 };

        var useCase = new SaveSettingsUseCase(repositoryMock.Object);
        await useCase.ExecuteAsync(settings);

        repositoryMock.Verify(x => x.SaveAsync(settings), Times.Once);
    }
}
