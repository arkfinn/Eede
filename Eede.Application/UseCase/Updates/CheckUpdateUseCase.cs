#nullable enable
using System.Threading.Tasks;
using Eede.Application.Infrastructure;

namespace Eede.Application.UseCase.Updates;

public class CheckUpdateUseCase
{
    private readonly IUpdateService _updateService;

    public CheckUpdateUseCase(IUpdateService updateService)
    {
        _updateService = updateService;
    }

    public async Task ExecuteAsync()
    {
        var hasUpdate = await _updateService.CheckForUpdatesAsync();
        if (hasUpdate)
        {
            await _updateService.DownloadUpdateAsync();
        }
    }
}
