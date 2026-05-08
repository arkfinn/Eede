using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Application.UseCase.Updates;
using Eede.Domain.SharedKernel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Eede.Presentation.ViewModels.General;

public partial class WelcomeViewModel : ViewModelBase, IDisposable
{
    public ObservableCollection<RecentFile> RecentFiles { get; } = new();

    public string Version => GetVersion();
    public string DisplayVersion => LatestVersion ?? Version;

    [Reactive] public partial UpdateStatus UpdateStatus { get; set; }
    [Reactive] public partial string? LatestVersion { get; set; }
    [ObservableAsProperty] private bool _isUpdateChecking;
    [ObservableAsProperty] private bool _isUpdateDownloading;
    [ObservableAsProperty] private bool _isUpdateReady;
    [ObservableAsProperty] private bool _isUpdateAvailable;
    [ObservableAsProperty] private string? _updateMessage;

    public ReactiveCommand<Unit, Unit> CreateNewPictureCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenPictureCommand { get; }
    public ReactiveCommand<string, string> OpenRecentFileCommand { get; }
    public ReactiveCommand<string, string> OpenUrlCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadRecentFilesCommand { get; }
    public ReactiveCommand<Unit, Unit> ApplyUpdateCommand { get; }
    public ReactiveCommand<Unit, Unit> ManualCheckUpdateCommand { get; }

    private readonly ISettingsRepository _settingsRepository;
    private readonly IExternalBrowserService _externalBrowserService;
    private readonly CheckUpdateUseCase? _checkUpdateUseCase;
    private readonly IUpdateService? _updateService;
    private readonly CompositeDisposable _disposables = new();

    public WelcomeViewModel(
        ISettingsRepository settingsRepository,
        IExternalBrowserService externalBrowserService,
        IUpdateService? updateService = null,
        CheckUpdateUseCase? checkUpdateUseCase = null)
    {
        _settingsRepository = settingsRepository;
        _externalBrowserService = externalBrowserService;
        _updateService = updateService;
        _checkUpdateUseCase = checkUpdateUseCase;

        CreateNewPictureCommand = ReactiveCommand.Create(() => { });
        OpenPictureCommand = ReactiveCommand.Create(() => { });
        OpenRecentFileCommand = ReactiveCommand.Create<string, string>(path => path);
        OpenUrlCommand = ReactiveCommand.CreateFromTask<string, string>(async url =>
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                await Task.Run(() =>
                {
                    _externalBrowserService.OpenUrl(url);
                });
            }
            return url;
        });

        LoadRecentFilesCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var settings = await _settingsRepository.LoadAsync();
            RecentFiles.Clear();
            foreach (var file in settings.RecentFiles)
            {
                RecentFiles.Add(file);
            }
        });

        var canApplyUpdate = this.WhenAnyValue(x => x.UpdateStatus)
            .Select(status => status == UpdateStatus.ReadyToApply);
        ApplyUpdateCommand = ReactiveCommand.Create(() =>
        {
            _updateService?.ApplyAndRestart();
        }, canApplyUpdate);

        var canCheckUpdate = this.WhenAnyValue(x => x.UpdateStatus)
            .Select(status => status == UpdateStatus.Idle || status == UpdateStatus.Error);
        ManualCheckUpdateCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (_checkUpdateUseCase != null)
            {
                await _checkUpdateUseCase.ExecuteAsync();
            }
        }, canCheckUpdate);

        _isUpdateCheckingHelper = null!;
        _isUpdateDownloadingHelper = null!;
        _isUpdateReadyHelper = null!;
        _isUpdateAvailableHelper = null!;
        _updateMessageHelper = null!;

        this.WhenAnyValue(x => x.UpdateStatus)
            .Select(x => x == UpdateStatus.Checking)
            .ToProperty(this, nameof(IsUpdateChecking), out _isUpdateCheckingHelper);

        this.WhenAnyValue(x => x.UpdateStatus)
            .Select(x => x == UpdateStatus.Downloading)
            .ToProperty(this, nameof(IsUpdateDownloading), out _isUpdateDownloadingHelper);

        this.WhenAnyValue(x => x.UpdateStatus)
            .Select(x => x == UpdateStatus.ReadyToApply)
            .ToProperty(this, nameof(IsUpdateReady), out _isUpdateReadyHelper);

        this.WhenAnyValue(x => x.UpdateStatus)
            .Select(x => x != UpdateStatus.Idle)
            .ToProperty(this, nameof(IsUpdateAvailable), out _isUpdateAvailableHelper);

        this.WhenAnyValue(x => x.UpdateStatus, x => x.LatestVersion)
            .Select(x => x.Item1 switch
            {
                UpdateStatus.Checking => "アップデートを確認中...",
                UpdateStatus.Downloading => $"最新バージョン ({x.Item2}) をダウンロード中...",
                UpdateStatus.ReadyToApply => $"新しいバージョン ({x.Item2}) の準備ができました",
                UpdateStatus.Error => "アップデートの確認に失敗しました",
                _ => ""
            })
            .ToProperty(this, nameof(UpdateMessage), out _updateMessageHelper);

        if (_updateService != null)
        {
            _disposables.Add(_updateService.StatusChanged
                .ObserveOn(ReactiveUI.Avalonia.AvaloniaScheduler.Instance)
                .Subscribe(status =>
                {
                    UpdateStatus = status;
                    LatestVersion = _updateService.LatestVersion;
                }));
        }

        _disposables.Add(this.WhenAnyValue(x => x.LatestVersion)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(DisplayVersion))));

        // 初期化時にアップデートチェックを開始（本来は非同期で実行）
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        if (_checkUpdateUseCase == null) return;

        await _checkUpdateUseCase.ExecuteAsync();
    }

    public void Dispose()
    {
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }

    private string GetVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        return fvi.FileVersion ?? "0.0.0";
    }
}
