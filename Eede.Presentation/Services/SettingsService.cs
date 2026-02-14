using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using System.Threading.Tasks;

namespace Eede.Presentation.Services;

public class SettingsService
{
    private readonly ISettingsRepository _repository;
    private AppSettings? _cachedSettings;

    public SettingsService(ISettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        if (_cachedSettings == null)
        {
            _cachedSettings = await _repository.LoadAsync();
        }
        return _cachedSettings;
    }

    public async Task SaveGridSizeAsync(int width, int height)
    {
        var settings = await GetSettingsAsync();
        settings.GridWidth = width;
        settings.GridHeight = height;
        await _repository.SaveAsync(settings);
    }
}
