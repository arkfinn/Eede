#nullable enable
using System;
using System.Threading.Tasks;
using Eede.Domain.SharedKernel;

namespace Eede.Application.Infrastructure;

public interface IUpdateService
{
    UpdateStatus Status { get; }
    IObservable<UpdateStatus> StatusChanged { get; }
    Task<bool> CheckForUpdatesAsync();
    Task DownloadUpdateAsync();
    void ApplyAndRestart();
}
