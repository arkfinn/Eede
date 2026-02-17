#nullable enable
using System.Threading.Tasks;
using Eede.Domain.SharedKernel;

namespace Eede.Application.Infrastructure;

public interface IUpdateService
{
    UpdateStatus Status { get; }
    Task<bool> CheckForUpdatesAsync();
    Task DownloadUpdateAsync();
    void ApplyAndRestart();
}
