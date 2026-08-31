using System;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Threading.Tasks;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;

namespace Eede.Infrastructure.Settings;

public partial class LocalStorageSettingsRepository : ISettingsRepository
{
    private readonly Func<string, string?>? _getter;
    private readonly Action<string, string>? _setter;
    private readonly string _storageKey;

    public LocalStorageSettingsRepository(
        Func<string, string?>? getter = null,
        Action<string, string>? setter = null,
        string storageKey = "eede_app_settings")
    {
        _getter = getter;
        _setter = setter;
        _storageKey = storageKey;
    }

    public Task<AppSettings> LoadAsync()
    {
        var defaultSettings = new AppSettings { GridWidth = 32, GridHeight = 32 };
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
                return Task.FromResult(defaultSettings);
            }

            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            return Task.FromResult(settings ?? defaultSettings);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"LocalStorageSettingsRepository.LoadAsync error: {ex.Message}");
            return Task.FromResult(defaultSettings);
        }
    }

    public Task<bool> SaveAsync(AppSettings settings)
    {
        try
        {
            string json = JsonSerializer.Serialize(settings);
            if (_setter != null)
            {
                _setter(_storageKey, json);
                return Task.FromResult(true);
            }

            if (OperatingSystem.IsBrowser())
            {
                JsInterop.SetItem(_storageKey, json);
                return Task.FromResult(true);
            }

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"LocalStorageSettingsRepository.SaveAsync error: {ex.Message}");
            return Task.FromResult(false);
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
