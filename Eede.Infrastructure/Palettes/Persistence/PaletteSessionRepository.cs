using Eede.Application.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eede.Infrastructure.Palettes.Persistence;

public class PaletteSessionRepository : IPaletteSessionRepository
{
    private readonly string _filePath;

    public PaletteSessionRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task SaveAsync(IEnumerable<string> filePaths)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(filePaths);
        await File.WriteAllTextAsync(_filePath, json);
    }

    public async Task<IEnumerable<string>> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return Array.Empty<string>();
        }

        try
        {
            using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            return await JsonSerializer.DeserializeAsync<IEnumerable<string>>(fs) ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}
