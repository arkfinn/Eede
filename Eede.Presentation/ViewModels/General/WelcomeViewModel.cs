using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reflection;
using ReactiveUI;
using RxVoid = ReactiveUI.Primitives.RxVoid;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Application.UseCase.Updates;
using Eede.Domain.ImageEditing.Recovery;
using Eede.Domain.SharedKernel;
using ReactiveUI.SourceGenerators;

namespace Eede.Presentation.ViewModels.General;

public partial class WelcomeViewModel : ViewModelBase, IDisposable
{
    public ObservableCollection<RecentFile> RecentFiles { get; } = new();

    public string Version => GetVersion();
    public string DisplayVersion => LatestVersion ?? Version;
    public bool IsUpdateSupported => _appUpdater?.IsSupported ?? false;

    [Reactive] public partial UpdateStatus UpdateStatus { get; set; }
    [Reactive] public partial string? LatestVersion { get; set; }
    [ObservableAsProperty] private bool _isUpdateChecking;
    [ObservableAsProperty] private bool _isUpdateDownloading;
    [ObservableAsProperty] private bool _isUpdateReady;
    [ObservableAsProperty] private bool _isUpdateAvailable;
    [ObservableAsProperty] private bool _isManualCheckVisible;
    [ObservableAsProperty] private string? _updateMessage;

    [Reactive] public partial bool HasPreviousSession { get; set; }
    [Reactive] public partial bool IsCrashRecovery { get; set; }
    [Reactive] public partial string PreviousSessionTitle { get; set; }
    [Reactive] public partial string PreviousSessionDescription { get; set; }

    public ReactiveCommand<RxVoid, RxVoid> CreateNewPictureCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> OpenPictureCommand { get; }
    public ReactiveCommand<string, string> OpenRecentFileCommand { get; }
    public ReactiveCommand<string, string> OpenUrlCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> LoadRecentFilesCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> ApplyUpdateCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> ManualCheckUpdateCommand { get; }
    [Reactive] public partial ReactiveCommand<RxVoid, RxVoid>? ResumeLastSessionCommand { get; set; }
    [Reactive] public partial ReactiveCommand<RxVoid, RxVoid>? DiscardLastSessionCommand { get; set; }

    public void SetPreviousSessionInfo(SessionSnapshot? metadata, bool isCrash)
    {
        if (metadata == null)
        {
            ClearPreviousSessionInfo();
            return;
        }

        HasPreviousSession = true;
        IsCrashRecovery = isCrash;
        PreviousSessionTitle = isCrash ? "前回の未保存作業を復元" : "前回の作業を再開";

        var count = metadata.Documents.Count;
        var timeStr = metadata.CreatedAt.ToLocalTime().ToString("yyyy/MM/dd HH:mm");
        var pullNote = metadata.PullState != null ? "（編集中エリアあり）" : "";
        PreviousSessionDescription = $"{count} 件のファイル{pullNote}・最終保存: {timeStr}";
    }

    public void ClearPreviousSessionInfo()
    {
        HasPreviousSession = false;
        IsCrashRecovery = false;
        PreviousSessionTitle = string.Empty;
        PreviousSessionDescription = string.Empty;
    }

    private readonly ISettingsRepository _settingsRepository;
    private readonly IExternalBrowserLauncher _browserLauncher;
    private readonly CheckUpdateUseCase? _checkUpdateUseCase;
    private readonly IAppUpdater? _appUpdater;
    private readonly CompositeDisposable _disposables = new();

    public WelcomeViewModel(
        ISettingsRepository settingsRepository,
        IExternalBrowserLauncher browserLauncher,
        IAppUpdater? appUpdater = null,
        CheckUpdateUseCase? checkUpdateUseCase = null)
    {
        _settingsRepository = settingsRepository;
        _browserLauncher = browserLauncher;
        _appUpdater = appUpdater;
        _checkUpdateUseCase = checkUpdateUseCase;

        PreviousSessionTitle = string.Empty;
        PreviousSessionDescription = string.Empty;
        ResumeLastSessionCommand = ReactiveCommand.Create(() => { });
        DiscardLastSessionCommand = ReactiveCommand.Create(() => { });
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
                    _browserLauncher.OpenUrl(url);
                });
            }
            return url;
        });

        LoadRecentFilesCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var settings = await _settingsRepository.LoadAsync();
            RecentFiles.Clear();
            if (settings?.RecentFiles != null)
            {
                foreach (var file in settings.RecentFiles)
                {
                    RecentFiles.Add(file);
                }
            }
        });

        var canApplyUpdate = this.WhenAnyValue(x => x.UpdateStatus)
            .Select(status => status == UpdateStatus.ReadyToApply);
        ApplyUpdateCommand = ReactiveCommand.Create(() =>
        {
            _appUpdater?.ApplyAndRestart();
        }, canApplyUpdate);

        var canCheckUpdate = this.WhenAnyValue(x => x.UpdateStatus)
            .Select(status => IsUpdateSupported && (status == UpdateStatus.Idle || status == UpdateStatus.Error));
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
        _isManualCheckVisibleHelper = null!;
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

        this.WhenAnyValue(x => x.UpdateStatus)
            .Select(status => IsUpdateSupported && status != UpdateStatus.ReadyToApply)
            .ToProperty(this, nameof(IsManualCheckVisible), out _isManualCheckVisibleHelper);

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

        if (_appUpdater != null)
        {
            _disposables.Add(_appUpdater.StatusChanged
                .ObserveOn(System.Reactive.Concurrency.ImmediateScheduler.Instance)
                .Subscribe(status =>
                {
                    UpdateStatus = status;
                    LatestVersion = _appUpdater.LatestVersion;
                }));
        }

        _disposables.Add(this.WhenAnyValue(x => x.LatestVersion)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(DisplayVersion))));

        // 初期化時にアップデートチェックを開始（本来は非同期で実行）
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        LoadRecentFilesCommand.Execute().Subscribe();

        if (_checkUpdateUseCase == null) return;

        await _checkUpdateUseCase.ExecuteAsync();
    }

    public void Dispose()
    {
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }

    private static string GetVersion()
    {
        var assembly = typeof(WelcomeViewModel).Assembly;

        // 1. MinVer 等で付与された InformationalVersion (メモリ内アセンブリメタデータ属性。WASM / Desktop 共通)
        var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(infoVersion))
        {
            var plusIndex = infoVersion.IndexOf('+');
            return plusIndex > 0 ? infoVersion[..plusIndex] : infoVersion;
        }

        // 2. 物理ファイルが存在するデスクトップ環境での FileVersionInfo
        if (!string.IsNullOrEmpty(assembly.Location))
        {
            try
            {
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                if (!string.IsNullOrWhiteSpace(fvi.FileVersion))
                {
                    return fvi.FileVersion;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[WelcomeViewModel] Failed to get FileVersionInfo: {ex.Message}");
            }
        }

        // 3. アセンブリバージョンへのフォールバック
        var version = assembly.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "0.0.0";
    }
}
