using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Presentation.ViewModels.General;
using Moq;
using NUnit.Framework;
using ReactiveUI;

namespace Eede.Presentation.Tests.ViewModels.General;

[TestFixture]
public class WelcomeViewModelTests
{
    private Mock<ISettingsRepository> _settingsRepoMock;
    private AppSettings _appSettings;

    [SetUp]
    public void Setup()
    {
        _settingsRepoMock = new Mock<ISettingsRepository>();
        _appSettings = new AppSettings();
        _appSettings.AddRecentFile("test1.png", System.DateTime.Now);
        _settingsRepoMock.Setup(r => r.LoadAsync()).ReturnsAsync(_appSettings);
    }

    [Test]
    public async Task RecentFiles_ShouldBeLoadedOnCreation()
    {
        var viewModel = new WelcomeViewModel(_settingsRepoMock.Object);
        
        await viewModel.LoadRecentFilesCommand.Execute().ToTask();

        Assert.That(viewModel.RecentFiles.Count, Is.EqualTo(1));
        Assert.That(viewModel.RecentFiles[0].Path, Is.EqualTo("test1.png"));
    }
}
