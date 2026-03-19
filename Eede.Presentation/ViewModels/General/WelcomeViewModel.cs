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
using ReactiveUI.Fody.Helpers;

namespace Eede.Presentation.ViewModels.General;

public class WelcomeViewModel : ViewModelBase, IDisposable
{
    public ObservableCollection<RecentFile> RecentFiles { get; } = new();

    public string Version => GetVersion();
    public string DisplayVersion => LatestVersion ?? Version;

    [Reactive] public UpdateStatus UpdateStatus { get; set; }
    [Reactive] public string? LatestVersion { get; set; }
    [ObservableAsProperty] public bool IsUpdateChecking { get; }
    [ObservableAsProperty] public bool IsUpdateDownloading { get; }
    [ObservableAsProperty] public bool IsUpdateReady { get; }
    [ObservableAsProperty] public bool IsUpdateAvailable { get; }
    [ObservableAsProperty] public string UpdateMessage { get; }

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

        this.WhenAnyValue(x => x.UpdateStatus)
            .Select(x => x == UpdateStatus.Checking)
            .ToPropertyEx(this, x => x.IsUpdateChecking);

        this.WhenAnyValue(x => x.UpdateStatus)
            .Select(x => x == UpdateStatus.Downloading)
            .ToPropertyEx(this, x => x.IsUpdateDownloading);

        this.WhenAnyValue(x => x.UpdateStatus)
            .Select(x => x == UpdateStatus.ReadyToApply)
            .ToPropertyEx(this, x => x.IsUpdateReady);

        this.WhenAnyValue(x => x.UpdateStatus)
            .Select(x => x != UpdateStatus.Idle)
            .ToPropertyEx(this, x => x.IsUpdateAvailable);

        this.WhenAnyValue(x => x.UpdateStatus, x => x.LatestVersion)
            .Select(x => x.Item1 switch
            {
                UpdateStatus.Checking => "アップデートを確認中...",
                UpdateStatus.Downloading => $"最新バージョン ({x.Item2}) をダウンロード中...",
                UpdateStatus.ReadyToApply => $"新しいバージョン ({x.Item2}) の準備ができました",
                UpdateStatus.Error => "アップデートの確認に失敗しました",
                _ => ""
            })
            .ToPropertyEx(this, x => x.UpdateMessage);

        if (_updateService != null)
        {
            _updateService.StatusChanged
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(status =>
                {
                    UpdateStatus = status;
                    LatestVersion = _updateService.LatestVersion;
                })
                .DisposeWith(_disposables);
        }

        this.WhenAnyValue(x => x.LatestVersion)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(DisplayVersion)))
            .DisposeWith(_disposables);

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
