using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Settings;

public interface ISaveSettingsUseCase
{
    Task ExecuteAsync(AppSettings settings);
}

public class SaveSettingsUseCase(ISettingsRepository repository) : ISaveSettingsUseCase
{
    public Task ExecuteAsync(AppSettings settings) => repository.SaveAsync(settings);
}
