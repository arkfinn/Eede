using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace Eede.Infrastructure.Updates;

public class UpdateManagerWrapper : IUpdateManagerWrapper
{
    private readonly UpdateManager _manager;

    public UpdateManagerWrapper(string githubUrl)
    {
        _manager = new UpdateManager(new GithubSource(githubUrl, null, false));
    }

    public bool IsInstalled => _manager.IsInstalled;
    public SemanticVersion? CurrentVersion => _manager.CurrentVersion;

    public Task<UpdateInfo?> CheckForUpdatesAsync() => _manager.CheckForUpdatesAsync();

    public Task DownloadUpdatesAsync(UpdateInfo updateInfo) => _manager.DownloadUpdatesAsync(updateInfo);

    public void ApplyUpdatesAndRestart(UpdateInfo updateInfo) => _manager.ApplyUpdatesAndRestart(updateInfo);
}
