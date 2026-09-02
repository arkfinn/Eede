using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Eede.Application.Infrastructure;
using Eede.Domain.SharedKernel;

namespace Eede.Infrastructure.Updates;

public class NullUpdateService : IUpdateService
{
    private readonly BehaviorSubject<UpdateStatus> _statusSubject = new(UpdateStatus.Idle);

    public bool IsSupported => false;

    public UpdateStatus Status => _statusSubject.Value;

    public IObservable<UpdateStatus> StatusChanged => _statusSubject.AsObservable();

    public string? LatestVersion => null;

    public Task<bool> CheckForUpdatesAsync()
    {
        return Task.FromResult(false);
    }

    public Task DownloadUpdateAsync()
    {
        return Task.CompletedTask;
    }

    public void ApplyAndRestart()
    {
        // No-op for web / unsupported platforms
    }

    public void Dispose()
    {
        _statusSubject.Dispose();
    }
}
