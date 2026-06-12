using Eede.Application.Settings;
using System.Threading.Tasks;

namespace Eede.Application.Infrastructure;

public interface ISettingsRepository
{
    Task<AppSettings> LoadAsync();
    Task<bool> SaveAsync(AppSettings settings);
}
