#nullable enable
using System.Threading.Tasks;
using Eede.Application.Infrastructure;

namespace Eede.Application.UseCase.Updates;

public class CheckUpdateUseCase
{
    private readonly IAppUpdater _appUpdater;

    public CheckUpdateUseCase(IAppUpdater appUpdater)
    {
        _appUpdater = appUpdater;
    }

    public async Task ExecuteAsync()
    {
        var hasUpdate = await _appUpdater.CheckForUpdatesAsync();
        if (hasUpdate)
        {
            await _appUpdater.DownloadUpdateAsync();
        }
    }
}
