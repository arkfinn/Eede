#nullable enable
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Eede.Application.Infrastructure;
using Eede.Domain.SharedKernel;
using Velopack;
using Velopack.Sources;

namespace Eede.Infrastructure.Updates;

public class VelopackUpdateService : IUpdateService, IDisposable
{
    public UpdateStatus Status { get; private set; } = UpdateStatus.Idle;

    private readonly BehaviorSubject<UpdateStatus> _statusSubject = new(UpdateStatus.Idle);
    public IObservable<UpdateStatus> StatusChanged => _statusSubject.AsObservable();

    private readonly string _githubUrl;
    private UpdateInfo? _updateInfo;

    public VelopackUpdateService(string githubUrl)
    {
        _githubUrl = githubUrl;
    }

    private void SetStatus(UpdateStatus status)
    {
        Status = status;
        _statusSubject.OnNext(status);
    }

    public async Task<bool> CheckForUpdatesAsync()
    {
        try
        {
            SetStatus(UpdateStatus.Checking);
            var mgr = new UpdateManager(new GithubSource(_githubUrl, null, false));
            _updateInfo = await mgr.CheckForUpdatesAsync();

            if (_updateInfo == null)
            {
                SetStatus(UpdateStatus.Idle);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            // インストールされていない環境（デバッグ時など）では UpdateManager が例外を投げる可能性があります。
            // その場合はエラー表示にせず、Idle ステータスに戻します。
            System.Diagnostics.Debug.WriteLine($"Velopack CheckForUpdatesAsync error: {ex.Message}");
            SetStatus(UpdateStatus.Idle);
            return false;
        }
    }

    public async Task DownloadUpdateAsync()
    {
        try
        {
            if (_updateInfo == null) return;
            SetStatus(UpdateStatus.Downloading);
            var mgr = new UpdateManager(new GithubSource(_githubUrl, null, false));
            await mgr.DownloadUpdatesAsync(_updateInfo);
            SetStatus(UpdateStatus.ReadyToApply);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Velopack DownloadUpdateAsync error: {ex.Message}");
            SetStatus(UpdateStatus.Error);
        }
    }

    public void ApplyAndRestart()
    {
        try
        {
            if (Status != UpdateStatus.ReadyToApply) return;
            var mgr = new UpdateManager(new GithubSource(_githubUrl, null, false));
            mgr.ApplyUpdatesAndRestart(_updateInfo);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Velopack ApplyAndRestart error: {ex.Message}");
            SetStatus(UpdateStatus.Error);
        }
    }

    public void Dispose()
    {
        _statusSubject.Dispose();
        GC.SuppressFinalize(this);
    }
}
