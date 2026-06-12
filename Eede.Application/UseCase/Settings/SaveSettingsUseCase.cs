using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Settings;

public interface ISaveSettingsUseCase
{
    Task<bool> ExecuteAsync(AppSettings settings);
}

public class SaveSettingsUseCase(ISettingsRepository repository) : ISaveSettingsUseCase
{
    public Task<bool> ExecuteAsync(AppSettings settings) => repository.SaveAsync(settings);
}
