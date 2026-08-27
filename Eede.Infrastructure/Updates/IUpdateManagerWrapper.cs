using System;
using System.Threading.Tasks;
using Velopack;

namespace Eede.Infrastructure.Updates;

public interface IUpdateManagerWrapper
{
    bool IsInstalled { get; }
    SemanticVersion? CurrentVersion { get; }
    Task<UpdateInfo?> CheckForUpdatesAsync();
    Task DownloadUpdatesAsync(UpdateInfo updateInfo);
    void ApplyUpdatesAndRestart(UpdateInfo updateInfo);
}
