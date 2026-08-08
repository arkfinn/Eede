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

    public IEnumerable<string> Load()
    {
        if (!File.Exists(_filePath))
        {
            return Array.Empty<string>();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<IEnumerable<string>>(json) ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}
