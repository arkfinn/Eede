#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Eede.Application.Recovery;
using Eede.Domain.ImageEditing.Recovery;

namespace Eede.Infrastructure.Recovery;

public partial class BrowserIndexedDbSessionStorage : ISessionStorage
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = true
    };

    private readonly Func<string, string, Task<bool>>? _customSave;
    private readonly Func<Task<string?>>? _customLoadLatest;
    private readonly Func<string, Task<string?>>? _customLoadPayload;
    private readonly Func<Task<bool>>? _customClear;
    private readonly Func<Task<bool>>? _customHasActive;
    private readonly Func<Task<bool>>? _customMarkCleanExit;
    private readonly Func<Task<bool>>? _customHasCleanExit;

    public BrowserIndexedDbSessionStorage(
        Func<string, string, Task<bool>>? customSave = null,
        Func<Task<string?>>? customLoadLatest = null,
        Func<string, Task<string?>>? customLoadPayload = null,
        Func<Task<bool>>? customClear = null,
        Func<Task<bool>>? customHasActive = null,
        Func<Task<bool>>? customMarkCleanExit = null,
        Func<Task<bool>>? customHasCleanExit = null)
    {
        _customSave = customSave;
        _customLoadLatest = customLoadLatest;
        _customLoadPayload = customLoadPayload;
        _customClear = customClear;
        _customHasActive = customHasActive;
        _customMarkCleanExit = customMarkCleanExit;
        _customHasCleanExit = customHasCleanExit;
    }

    public async Task SaveSnapshotAsync(SessionSnapshot snapshot, IReadOnlyDictionary<string, byte[]> imagePayloads, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(imagePayloads);

        try
        {
            string snapshotJson = JsonSerializer.Serialize(snapshot, _jsonOptions);

            var payloadsDict = new Dictionary<string, string>();
            foreach (var kvp in imagePayloads)
            {
                payloadsDict[kvp.Key] = Convert.ToBase64String(kvp.Value);
            }
            string payloadsJson = JsonSerializer.Serialize(payloadsDict);

            if (_customSave != null)
            {
                await _customSave(snapshotJson, payloadsJson);
                return;
            }

            if (OperatingSystem.IsBrowser())
            {
                await JsInterop.SaveSnapshot(snapshotJson, payloadsJson);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"[BrowserIndexedDbSessionStorage] SaveSnapshotAsync error: {ex}");
        }
    }

    public async Task<SessionSnapshot?> LoadLatestSnapshotAsync(CancellationToken ct = default)
    {
        try
        {
            string? json = null;
            if (_customLoadLatest != null)
            {
                json = await _customLoadLatest();
            }
            else if (OperatingSystem.IsBrowser())
            {
                json = await JsInterop.LoadLatestSnapshot();
            }

            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonSerializer.Deserialize<SessionSnapshot>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"[BrowserIndexedDbSessionStorage] LoadLatestSnapshotAsync error: {ex}");
            return null;
        }
    }

    public async Task<byte[]?> LoadImagePayloadAsync(string payloadRef, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadRef);

        try
        {
            string? base64 = null;
            if (_customLoadPayload != null)
            {
                base64 = await _customLoadPayload(payloadRef);
            }
            else if (OperatingSystem.IsBrowser())
            {
                base64 = await JsInterop.LoadPayload(payloadRef);
            }

            if (string.IsNullOrWhiteSpace(base64)) return null;
            return Convert.FromBase64String(base64);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"[BrowserIndexedDbSessionStorage] LoadImagePayloadAsync error for '{payloadRef}': {ex}");
            return null;
        }
    }

    public async Task ClearSessionAsync(CancellationToken ct = default)
    {
        try
        {
            if (_customClear != null)
            {
                await _customClear();
                return;
            }

            if (OperatingSystem.IsBrowser())
            {
                await JsInterop.ClearSession();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"[BrowserIndexedDbSessionStorage] ClearSessionAsync error: {ex}");
        }
    }

    public async Task<bool> HasActiveSessionAsync(CancellationToken ct = default)
    {
        try
        {
            if (_customHasActive != null)
            {
                return await _customHasActive();
            }

            if (OperatingSystem.IsBrowser())
            {
                return await JsInterop.HasActiveSession();
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"[BrowserIndexedDbSessionStorage] HasActiveSessionAsync error: {ex}");
            return false;
        }
    }

    public async Task<bool> HasCleanExitMarkerAsync(CancellationToken ct = default)
    {
        try
        {
            if (_customHasCleanExit != null)
            {
                return await _customHasCleanExit();
            }

            if (OperatingSystem.IsBrowser())
            {
                return await JsInterop.HasCleanExit();
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"[BrowserIndexedDbSessionStorage] HasCleanExitMarkerAsync error: {ex}");
            return false;
        }
    }

    public async Task MarkCleanExitAsync(CancellationToken ct = default)
    {
        try
        {
            if (_customMarkCleanExit != null)
            {
                await _customMarkCleanExit();
                return;
            }

            if (OperatingSystem.IsBrowser())
            {
                await JsInterop.MarkCleanExit();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"[BrowserIndexedDbSessionStorage] MarkCleanExitAsync error: {ex}");
        }
    }

    private static partial class JsInterop
    {
        [JSImport("globalThis.eedeSessionDb.saveSnapshot")]
        internal static partial Task<bool> SaveSnapshot(string snapshotJson, string payloadsJson);

        [JSImport("globalThis.eedeSessionDb.loadLatestSnapshot")]
        internal static partial Task<string?> LoadLatestSnapshot();

        [JSImport("globalThis.eedeSessionDb.loadPayload")]
        internal static partial Task<string?> LoadPayload(string payloadRef);

        [JSImport("globalThis.eedeSessionDb.clearSession")]
        internal static partial Task<bool> ClearSession();

        [JSImport("globalThis.eedeSessionDb.hasActiveSession")]
        internal static partial Task<bool> HasActiveSession();

        [JSImport("globalThis.eedeSessionDb.markCleanExit")]
        internal static partial Task<bool> MarkCleanExit();

        [JSImport("globalThis.eedeSessionDb.hasCleanExit")]
        internal static partial Task<bool> HasCleanExit();
    }
}
