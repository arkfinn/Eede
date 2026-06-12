using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eede.Infrastructure.Settings;

public class JsonSettingsRepository : ISettingsRepository
{
    private readonly string _filePath;

    public JsonSettingsRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<AppSettings> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new AppSettings { GridWidth = 32, GridHeight = 32 };
        }

        try
        {
            using var stream = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<AppSettings>(stream) ?? new AppSettings { GridWidth = 32, GridHeight = 32 };
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"Failed to load settings: {ex.Message}");
            return new AppSettings { GridWidth = 32, GridHeight = 32 };
        }
    }

    public async Task<bool> SaveAsync(AppSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, settings);
            return true;
        }
        catch (System.Exception ex)
        {
            // 保存失敗時はfalseを返して呼び出し元に通知する
            System.Diagnostics.Trace.WriteLine($"Failed to save settings: {ex.Message}");
            return false;
        }
    }
}
