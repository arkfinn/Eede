using NUnit.Framework;
using Eede.Application.Settings;
using Eede.Infrastructure.Settings;
using System.IO;
using System.Threading.Tasks;

namespace Eede.Infrastructure.Tests.Settings;

[TestFixture]
public class JsonSettingsRepositoryTests
{
    private string _testFilePath;

    [SetUp]
    public void SetUp()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [Test]
    public async Task LoadAsync_ShouldReturnDefaultSettings_WhenFileDoesNotExist()
    {
        var repository = new JsonSettingsRepository(_testFilePath);
        var settings = await repository.LoadAsync();

        Assert.That(settings, Is.Not.Null);
        Assert.That(settings.GridWidth, Is.EqualTo(32)); // Default
        Assert.That(settings.GridHeight, Is.EqualTo(32)); // Default
    }

    [Test]
    public async Task SaveAndLoadTest()
    {
        var repository = new JsonSettingsRepository(_testFilePath);
        var settings = new AppSettings { GridWidth = 48, GridHeight = 64 };

        await repository.SaveAsync(settings);
        var loaded = await repository.LoadAsync();

        Assert.That(loaded.GridWidth, Is.EqualTo(48));
        Assert.That(loaded.GridHeight, Is.EqualTo(64));
    }
}
