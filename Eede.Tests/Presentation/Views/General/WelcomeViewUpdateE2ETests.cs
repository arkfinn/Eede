using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.VisualTree;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Application.UseCase.Updates;
using Eede.Domain.SharedKernel;
using Eede.Presentation.ViewModels.General;
using Eede.Presentation.Views.General;
using Moq;
using NUnit.Framework;

namespace Eede.Presentation.Tests.Views.General;

[TestFixture]
public class WelcomeViewUpdateE2ETests
{
    private Mock<ISettingsRepository> _settingsRepoMock = default!;
    private Mock<IExternalBrowserLauncher> _browserLauncherMock = default!;
    private Mock<IAppUpdater> _appUpdaterMock = default!;
    private BehaviorSubject<UpdateStatus> _statusSubject = default!;
    private CheckUpdateUseCase _checkUpdateUseCase = default!;
    private Window _window = default!;
    private WelcomeView _welcomeView = default!;
    private WelcomeViewModel _viewModel = default!;

    [SetUp]
    public void SetUp()
    {
        _settingsRepoMock = new Mock<ISettingsRepository>();
        _settingsRepoMock.Setup(r => r.LoadAsync()).ReturnsAsync(new AppSettings());

        _browserLauncherMock = new Mock<IExternalBrowserLauncher>();
        _appUpdaterMock = new Mock<IAppUpdater>();
        _statusSubject = new BehaviorSubject<UpdateStatus>(UpdateStatus.Idle);
        _appUpdaterMock.SetupGet(s => s.StatusChanged).Returns(_statusSubject);
        _appUpdaterMock.SetupGet(s => s.LatestVersion).Returns("1.2.0");
        _appUpdaterMock.SetupGet(s => s.IsSupported).Returns(true);

        _checkUpdateUseCase = new CheckUpdateUseCase(_appUpdaterMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _window?.Close();
        _statusSubject?.Dispose();
        _viewModel?.Dispose();
    }

    private void InitializeView(WelcomeViewModel viewModel)
    {
        _viewModel = viewModel;
        _welcomeView = new WelcomeView
        {
            DataContext = _viewModel
        };
        _window = new Window
        {
            Content = _welcomeView,
            Width = 800,
            Height = 600
        };
        _window.Show();
    }

    [AvaloniaTest]
    public async Task UpdateFlow_WhenUpdateAvailable_ShouldTransitionToReadyToApply_AndShowApplyButton()
    {
        // 1. Arrange: アップデートが存在する場合のモック設定
        _appUpdaterMock.Setup(s => s.CheckForUpdatesAsync())
            .Callback(() => _statusSubject.OnNext(UpdateStatus.Checking))
            .ReturnsAsync(true);

        _appUpdaterMock.Setup(s => s.DownloadUpdateAsync())
            .Callback(() => _statusSubject.OnNext(UpdateStatus.Downloading))
            .Returns(Task.CompletedTask)
            .Callback(() => _statusSubject.OnNext(UpdateStatus.ReadyToApply));

        var vm = new WelcomeViewModel(_settingsRepoMock.Object, _browserLauncherMock.Object, _appUpdaterMock.Object, _checkUpdateUseCase);
        InitializeView(vm);

        // 初期化時の非同期実行（InitializeAsync）の完了を待機
        for (int i = 0; i < 50; i++)
        {
            if (vm.UpdateStatus == UpdateStatus.ReadyToApply) break;
            await Task.Delay(20);
        }

        // 2. Assert: ViewModel のステータスとメッセージ
        Assert.That(vm.UpdateStatus, Is.EqualTo(UpdateStatus.ReadyToApply));
        Assert.That(vm.IsUpdateReady, Is.True);
        Assert.That(vm.UpdateMessage, Does.Contain("新しいバージョン (1.2.0) の準備ができました"));

        // 3. Assert: View の UI 要素（ボタン）の表示状態
        var buttons = _welcomeView.GetVisualDescendants().OfType<Button>().ToList();
        var applyButton = buttons.FirstOrDefault(b => b.Command == vm.ApplyUpdateCommand);

        Assert.That(applyButton, Is.Not.Null, "再起動適用ボタンが存在すること");
        Assert.That(applyButton!.IsVisible, Is.True, "再起動適用ボタンが表示されていること");

        // 4. Act: 適用ボタンを押下（コマンド実行）
        applyButton.Command.Execute(null);

        // 5. Assert: ApplyAndRestart が呼ばれたことを検証
        _appUpdaterMock.Verify(s => s.ApplyAndRestart(), Times.Once);
    }

    [AvaloniaTest]
    public async Task UpdateFlow_WhenNoUpdate_ShouldShowUpToDateMessage_AndManualCheckButton()
    {
        // 1. Arrange: アップデートがない場合
        _appUpdaterMock.Setup(s => s.CheckForUpdatesAsync())
            .Callback(() => _statusSubject.OnNext(UpdateStatus.Checking))
            .ReturnsAsync(false)
            .Callback(() => _statusSubject.OnNext(UpdateStatus.Idle));

        var vm = new WelcomeViewModel(_settingsRepoMock.Object, _browserLauncherMock.Object, _appUpdaterMock.Object, _checkUpdateUseCase);
        InitializeView(vm);

        for (int i = 0; i < 50; i++)
        {
            if (vm.UpdateStatus == UpdateStatus.Idle) break;
            await Task.Delay(20);
        }

        // 2. Assert
        Assert.That(vm.UpdateStatus, Is.EqualTo(UpdateStatus.Idle));
        Assert.That(vm.IsUpdateAvailable, Is.False);

        var buttons = _welcomeView.GetVisualDescendants().OfType<Button>().ToList();
        var manualCheckButton = buttons.FirstOrDefault(b => b.Command == vm.ManualCheckUpdateCommand);

        Assert.That(manualCheckButton, Is.Not.Null);
        Assert.That(manualCheckButton!.IsVisible, Is.True);

        // 3. Act: 手動チェックボタンを押下
        _appUpdaterMock.Invocations.Clear();
        _appUpdaterMock.Setup(s => s.CheckForUpdatesAsync()).ReturnsAsync(false);

        manualCheckButton.Command.Execute(null);
        await Task.Delay(50);

        // 4. Assert: 再度 CheckForUpdatesAsync が呼ばれたこと
        _appUpdaterMock.Verify(s => s.CheckForUpdatesAsync(), Times.Once);
    }

    [AvaloniaTest]
    public async Task UpdateFlow_WhenErrorOccurs_ShouldShowErrorMessage_AndRetryButton()
    {
        // 1. Arrange: エラー発生時
        _appUpdaterMock.Setup(s => s.CheckForUpdatesAsync())
            .Callback(() => _statusSubject.OnNext(UpdateStatus.Checking))
            .ThrowsAsync(new System.Net.Http.HttpRequestException("Network failure"));

        var vm = new WelcomeViewModel(_settingsRepoMock.Object, _browserLauncherMock.Object, _appUpdaterMock.Object, _checkUpdateUseCase);
        InitializeView(vm);

        // エラーステータスへ通知
        _statusSubject.OnNext(UpdateStatus.Error);
        await Task.Delay(20);

        // 2. Assert: エラーメッセージとリトライボタンの表示
        Assert.That(vm.UpdateStatus, Is.EqualTo(UpdateStatus.Error));
        Assert.That(vm.UpdateMessage, Does.Contain("アップデートの確認に失敗しました"));
        Assert.That(vm.IsUpdateReady, Is.False);

        var buttons = _welcomeView.GetVisualDescendants().OfType<Button>().ToList();
        var retryButton = buttons.FirstOrDefault(b => b.Command == vm.ManualCheckUpdateCommand);

        Assert.That(retryButton, Is.Not.Null);
        Assert.That(retryButton!.IsVisible, Is.True);

        // 3. Act: リトライボタンを押下
        _appUpdaterMock.Invocations.Clear();
        _appUpdaterMock.Setup(s => s.CheckForUpdatesAsync()).ReturnsAsync(false);

        retryButton.Command.Execute(null);
        await Task.Delay(50);

        _appUpdaterMock.Verify(s => s.CheckForUpdatesAsync(), Times.Once);
    }

    [AvaloniaTest]
    public async Task WhenUpdateNotSupported_ManualCheckButtonShouldBeHidden()
    {
        // 1. Arrange: アップデート非対応（Web版 / NullAppUpdater）のモック
        _appUpdaterMock.SetupGet(s => s.IsSupported).Returns(false);
        _appUpdaterMock.Setup(s => s.CheckForUpdatesAsync()).ReturnsAsync(false);

        var vm = new WelcomeViewModel(_settingsRepoMock.Object, _browserLauncherMock.Object, _appUpdaterMock.Object, _checkUpdateUseCase);
        InitializeView(vm);

        await Task.Delay(50);

        // 2. Assert: ViewModel で IsUpdateSupported が false であること
        Assert.That(vm.IsUpdateSupported, Is.False);
        Assert.That(vm.IsUpdateAvailable, Is.False);

        // 3. Assert: View の手動チェックボタンがすべて非表示であること
        var buttons = _welcomeView.GetVisualDescendants().OfType<Button>().ToList();
        var manualCheckButtons = buttons.Where(b => b.Command == vm.ManualCheckUpdateCommand).ToList();

        Assert.That(manualCheckButtons, Is.Not.Empty);
        Assert.That(manualCheckButtons, Has.All.Property("IsVisible").EqualTo(false), "Web版（非対応環境）では手動チェックリンクがすべて非表示であること");
    }
}

