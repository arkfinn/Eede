using System;
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
        catch (Exception ex) when (ex is JsonException || ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException)
        {
            System.Diagnostics.Trace.WriteLine("Failed to load settings securely.");
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
        catch (Exception ex) when (ex is JsonException || ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException)
        {
            // 保存失敗時はfalseを返して呼び出し元に通知する
            System.Diagnostics.Trace.WriteLine("Failed to save settings securely.");
            return false;
        }
    }
}
