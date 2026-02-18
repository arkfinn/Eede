using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Application.UseCase.Updates;
using Eede.Domain.SharedKernel;
using Eede.Presentation.ViewModels.General;
using Moq;
using NUnit.Framework;
using ReactiveUI;

namespace Eede.Presentation.Tests.ViewModels.General;

[TestFixture]
public class WelcomeViewModelTests
{
    private Mock<ISettingsRepository> _settingsRepoMock;
    private Mock<IUpdateService> _updateServiceMock;
    private BehaviorSubject<UpdateStatus> _statusSubject;
    private AppSettings _appSettings;
    private CheckUpdateUseCase _checkUpdateUseCase;

    [SetUp]
    public void Setup()
    {
        _settingsRepoMock = new Mock<ISettingsRepository>();
        _updateServiceMock = new Mock<IUpdateService>();
        _statusSubject = new BehaviorSubject<UpdateStatus>(UpdateStatus.Idle);
        _updateServiceMock.SetupGet(s => s.StatusChanged).Returns(_statusSubject);

        _appSettings = new AppSettings();
        _appSettings.AddRecentFile("test1.png", System.DateTime.Now);
        _settingsRepoMock.Setup(r => r.LoadAsync()).ReturnsAsync(_appSettings);

        // ユースケースのセットアップ
        _checkUpdateUseCase = new CheckUpdateUseCase(_updateServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _statusSubject.Dispose();
    }

    [Test]
    public async Task LoadRecentFiles_ShouldRefreshList()
    {
        var viewModel = new WelcomeViewModel(_settingsRepoMock.Object, _updateServiceMock.Object, _checkUpdateUseCase);
        
        await viewModel.LoadRecentFilesCommand.Execute().ToTask();
        Assert.That(viewModel.RecentFiles.Count, Is.EqualTo(1));

        // 設定を更新して再ロード
        _appSettings.AddRecentFile("test2.png", System.DateTime.Now);
        await viewModel.LoadRecentFilesCommand.Execute().ToTask();

        Assert.That(viewModel.RecentFiles.Count, Is.EqualTo(2));
        Assert.That(viewModel.RecentFiles[0].Path, Is.EqualTo("test2.png"));
    }

    [Test]
    public void InitialStatus_ShouldBeIdle()
    {
        // 初期状態は Idle であることを検証
        var viewModel = new WelcomeViewModel(_settingsRepoMock.Object, _updateServiceMock.Object, _checkUpdateUseCase);
        Assert.That(viewModel.UpdateStatus, Is.EqualTo(UpdateStatus.Idle));
    }

    [Test]
    public void CheckUpdate_ShouldUpdateStatus()
    {
        var viewModel = new WelcomeViewModel(_settingsRepoMock.Object, _updateServiceMock.Object, _checkUpdateUseCase);

        // サービス側のステータスが変更されたら反映されることを検証
        _statusSubject.OnNext(UpdateStatus.Downloading);

        Assert.That(viewModel.UpdateStatus, Is.EqualTo(UpdateStatus.Downloading));
    }

    [Test]
    public void ApplyUpdateCommand_ShouldCallApplyAndRestart()
    {
        var viewModel = new WelcomeViewModel(_settingsRepoMock.Object, _updateServiceMock.Object, _checkUpdateUseCase);

        // コマンドが実行可能になるステータスに変更
        _statusSubject.OnNext(UpdateStatus.ReadyToApply);

        viewModel.ApplyUpdateCommand.Execute().Subscribe();

        _updateServiceMock.Verify(x => x.ApplyAndRestart(), Times.Once);
    }
}
