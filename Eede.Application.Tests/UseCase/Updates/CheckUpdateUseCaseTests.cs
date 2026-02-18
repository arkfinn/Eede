#nullable enable
using NUnit.Framework;
using Moq;
using Eede.Application.Infrastructure;
using Eede.Application.UseCase.Updates;
using System.Threading.Tasks;

namespace Eede.Application.Tests.UseCase.Updates;

[TestFixture]
public class CheckUpdateUseCaseTests
{
    private Mock<IUpdateService> _updateServiceMock;
    private CheckUpdateUseCase _useCase;

    [SetUp]
    public void SetUp()
    {
        _updateServiceMock = new Mock<IUpdateService>();
        // CheckUpdateUseCase is not defined yet, so this will fail to compile.
        _useCase = new CheckUpdateUseCase(_updateServiceMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_ShouldCallCheckForUpdates()
    {
        _updateServiceMock.Setup(x => x.CheckForUpdatesAsync()).ReturnsAsync(false);

        await _useCase.ExecuteAsync();

        _updateServiceMock.Verify(x => x.CheckForUpdatesAsync(), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldCallDownloadUpdate_WhenUpdateAvailable()
    {
        _updateServiceMock.Setup(x => x.CheckForUpdatesAsync()).ReturnsAsync(true);

        await _useCase.ExecuteAsync();

        _updateServiceMock.Verify(x => x.DownloadUpdateAsync(), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldNotCallDownloadUpdate_WhenUpdateNotAvailable()
    {
        _updateServiceMock.Setup(x => x.CheckForUpdatesAsync()).ReturnsAsync(false);

        await _useCase.ExecuteAsync();

        _updateServiceMock.Verify(x => x.DownloadUpdateAsync(), Times.Never);
    }
}
