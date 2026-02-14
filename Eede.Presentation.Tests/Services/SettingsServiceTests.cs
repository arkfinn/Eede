using NUnit.Framework;
using Moq;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Presentation.Services;
using System.Threading.Tasks;

namespace Eede.Presentation.Tests.Services;

[TestFixture]
public class SettingsServiceTests
{
    private Mock<ISettingsRepository> _repositoryMock;
    private SettingsService _service;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<ISettingsRepository>();
        _service = new SettingsService(_repositoryMock.Object);
    }

    [Test]
    public async Task GetSettingsAsync_ShouldLoadFromRepository()
    {
        var expected = new AppSettings { GridWidth = 16, GridHeight = 16 };
        _repositoryMock.Setup(x => x.LoadAsync()).ReturnsAsync(expected);

        var actual = await _service.GetSettingsAsync();

        Assert.That(actual.GridWidth, Is.EqualTo(expected.GridWidth));
        Assert.That(actual.GridHeight, Is.EqualTo(expected.GridHeight));
        _repositoryMock.Verify(x => x.LoadAsync(), Times.Once);
    }

    [Test]
    public async Task SaveGridSizeAsync_ShouldUpdateAndSave()
    {
        var initial = new AppSettings { GridWidth = 32, GridHeight = 32 };
        _repositoryMock.Setup(x => x.LoadAsync()).ReturnsAsync(initial);

        await _service.SaveGridSizeAsync(16, 24);

        _repositoryMock.Verify(x => x.SaveAsync(It.Is<AppSettings>(s => s.GridWidth == 16 && s.GridHeight == 24)), Times.Once);
    }
}
