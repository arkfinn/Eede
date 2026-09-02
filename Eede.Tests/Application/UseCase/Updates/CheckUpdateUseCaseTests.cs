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
    private Mock<IAppUpdater> _appUpdaterMock;
    private CheckUpdateUseCase _useCase;

    [SetUp]
    public void SetUp()
    {
        _appUpdaterMock = new Mock<IAppUpdater>();
        _useCase = new CheckUpdateUseCase(_appUpdaterMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_ShouldCallCheckForUpdates()
    {
        _appUpdaterMock.Setup(x => x.CheckForUpdatesAsync()).ReturnsAsync(false);

        await _useCase.ExecuteAsync();

        _appUpdaterMock.Verify(x => x.CheckForUpdatesAsync(), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldCallDownloadUpdate_WhenUpdateAvailable()
    {
        _appUpdaterMock.Setup(x => x.CheckForUpdatesAsync()).ReturnsAsync(true);

        await _useCase.ExecuteAsync();

        _appUpdaterMock.Verify(x => x.DownloadUpdateAsync(), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldNotCallDownloadUpdate_WhenUpdateNotAvailable()
    {
        _appUpdaterMock.Setup(x => x.CheckForUpdatesAsync()).ReturnsAsync(false);

        await _useCase.ExecuteAsync();

        _appUpdaterMock.Verify(x => x.DownloadUpdateAsync(), Times.Never);
    }
}
