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

    [Reactive] public UpdateStatus UpdateStatus { get; set; }
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

    private readonly ISettingsRepository _settingsRepository;
    private readonly CheckUpdateUseCase? _checkUpdateUseCase;
    private readonly IUpdateService? _updateService;
    private readonly CompositeDisposable _disposables = new();

    public WelcomeViewModel(ISettingsRepository settingsRepository, IUpdateService? updateService = null, CheckUpdateUseCase? checkUpdateUseCase = null)
    {
        _settingsRepository = settingsRepository;
        _updateService = updateService;
        _checkUpdateUseCase = checkUpdateUseCase;

        CreateNewPictureCommand = ReactiveCommand.Create(() => { });
        OpenPictureCommand = ReactiveCommand.Create(() => { });
        OpenRecentFileCommand = ReactiveCommand.Create<string, string>(path => path);
        OpenUrlCommand = ReactiveCommand.CreateFromTask<string, string>(async url =>
        {
            await Task.Run(() =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            });
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

        this.WhenAnyValue(x => x.UpdateStatus)
            .Select(x => x switch
            {
                UpdateStatus.Checking => "アップデートを確認中...",
                UpdateStatus.Downloading => "最新バージョンをダウンロード中...",
                UpdateStatus.ReadyToApply => "新しいバージョンの準備ができました",
                UpdateStatus.Error => "アップデートの確認に失敗しました",
                _ => ""
            })
            .ToPropertyEx(this, x => x.UpdateMessage);

        if (_updateService != null)
        {
            _updateService.StatusChanged
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(status => UpdateStatus = status)
                .DisposeWith(_disposables);
        }

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
