using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Threading.Tasks;
using Eede.Application.Infrastructure;

namespace Eede.Infrastructure.Palettes.Persistence;

public partial class LocalStoragePaletteSessionRepository : IPaletteSessionRepository
{
    private readonly Func<string, string?>? _getter;
    private readonly Action<string, string>? _setter;
    private readonly string _storageKey;

    public LocalStoragePaletteSessionRepository(
        Func<string, string?>? getter = null,
        Action<string, string>? setter = null,
        string storageKey = "eede_palette_session")
    {
        _getter = getter;
        _setter = setter;
        _storageKey = storageKey;
    }

    public Task<IEnumerable<string>> LoadAsync()
    {
        try
        {
            string? json = null;
            if (_getter != null)
            {
                json = _getter(_storageKey);
            }
            else if (OperatingSystem.IsBrowser())
            {
                json = JsInterop.GetItem(_storageKey);
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }

            var items = JsonSerializer.Deserialize<IEnumerable<string>>(json);
            return Task.FromResult(items ?? Enumerable.Empty<string>());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"LocalStoragePaletteSessionRepository.LoadAsync error: {ex.Message}");
            return Task.FromResult(Enumerable.Empty<string>());
        }
    }

    public Task SaveAsync(IEnumerable<string> filePaths)
    {
        try
        {
            string json = JsonSerializer.Serialize(filePaths);
            if (_setter != null)
            {
                _setter(_storageKey, json);
                return Task.CompletedTask;
            }

            if (OperatingSystem.IsBrowser())
            {
                JsInterop.SetItem(_storageKey, json);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"LocalStoragePaletteSessionRepository.SaveAsync error: {ex.Message}");
            return Task.CompletedTask;
        }
    }

    private static partial class JsInterop
    {
        [JSImport("globalThis.localStorage.getItem")]
        internal static partial string? GetItem(string key);

        [JSImport("globalThis.localStorage.setItem")]
        internal static partial void SetItem(string key, string value);
    }
}
