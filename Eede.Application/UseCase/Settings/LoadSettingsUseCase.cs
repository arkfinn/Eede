using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Settings;

public interface ILoadSettingsUseCase
{
    Task<AppSettings> ExecuteAsync();
}

public class LoadSettingsUseCase(ISettingsRepository repository) : ILoadSettingsUseCase
{
    public Task<AppSettings> ExecuteAsync() => repository.LoadAsync();
}
