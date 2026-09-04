using System.Threading.Tasks;
using Eede.Application.Infrastructure;
using Eede.Application.UseCase.Updates;
using Moq;
using NUnit.Framework;

namespace Eede.Tests.Application.UseCase.Updates;

[TestFixture]
public class CheckUpdateUseCaseTests
{
    private Mock<IAppUpdater> _appUpdaterMock = default!;
    private CheckUpdateUseCase _useCase = default!;

    [SetUp]
    public void SetUp()
    {
        _appUpdaterMock = new Mock<IAppUpdater>();
        _useCase = new CheckUpdateUseCase(_appUpdaterMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_WhenUpdateIsAvailable_DownloadsUpdate()
    {
        _appUpdaterMock.Setup(u => u.CheckForUpdatesAsync()).ReturnsAsync(true);

        await _useCase.ExecuteAsync();

        _appUpdaterMock.Verify(u => u.CheckForUpdatesAsync(), Times.Once);
        _appUpdaterMock.Verify(u => u.DownloadUpdateAsync(), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WhenUpdateIsNotAvailable_DoesNotDownloadUpdate()
    {
        _appUpdaterMock.Setup(u => u.CheckForUpdatesAsync()).ReturnsAsync(false);

        await _useCase.ExecuteAsync();

        _appUpdaterMock.Verify(u => u.CheckForUpdatesAsync(), Times.Once);
        _appUpdaterMock.Verify(u => u.DownloadUpdateAsync(), Times.Never);
    }
}
