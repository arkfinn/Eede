#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Eede.Application.Recovery;
using Eede.Domain.ImageEditing.Recovery;

namespace Eede.Infrastructure.Recovery;

public class LocalFileSessionStorage : ISessionStorage
{
    private readonly string _baseDirectory;
    private readonly string _currentDirectory;
    private readonly string _stagingDirectory;
    private readonly string _backupDirectory;
    private readonly string _tempDirectory;
    private readonly string _cleanExitMarkerPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public string BaseDirectory => _baseDirectory;

    public LocalFileSessionStorage(string baseDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);
        _baseDirectory = baseDirectory;
        _currentDirectory = Path.Combine(_baseDirectory, "current");
        _stagingDirectory = Path.Combine(_baseDirectory, "staging");
        _backupDirectory = Path.Combine(_baseDirectory, "backup");
        _tempDirectory = Path.Combine(_baseDirectory, "temp");
        _cleanExitMarkerPath = Path.Combine(_baseDirectory, "clean_exit.marker");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            IncludeFields = true
        };
    }

    public LocalFileSessionStorage()
        : this(GetDefaultDirectory())
    {
    }

    public static string GetDefaultDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrWhiteSpace(appData))
        {
            appData = Path.GetTempPath();
        }
        var pid = Environment.ProcessId;
        return Path.Combine(appData, "Eede", "recovery", $"pid_{pid}");
    }

    public static void ValidatePayloadRef(string payloadRef)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadRef);

        if (payloadRef.Contains("..") || payloadRef.Contains('/') || payloadRef.Contains('\\'))
        {
            throw new ArgumentException($"Invalid payload reference '{payloadRef}': path traversal characters are not allowed.", nameof(payloadRef));
        }

        foreach (char c in payloadRef)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c != '_' && c != '-' && c != '.')
            {
                throw new ArgumentException($"Invalid payload reference '{payloadRef}': character '{c}' is not allowed.", nameof(payloadRef));
            }
        }
    }

    public virtual async Task SaveSnapshotAsync(
        SessionSnapshot snapshot,
        IReadOnlyDictionary<string, byte[]> imagePayloads,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(imagePayloads);

        foreach (var (payloadRef, data) in imagePayloads)
        {
            ValidatePayloadRef(payloadRef);
            if (data is null)
            {
                throw new ArgumentException($"Payload data for '{payloadRef}' cannot be null.", nameof(imagePayloads));
            }
        }

        ct.ThrowIfCancellationRequested();

        if (!Directory.Exists(_baseDirectory))
        {
            Directory.CreateDirectory(_baseDirectory);
        }

        // 1. temp ディレクトリを準備
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
        Directory.CreateDirectory(_tempDirectory);

        try
        {
            // 2. temp ディレクトリに書き出し
            await WriteSnapshotAndPayloadsAsync(_tempDirectory, snapshot, imagePayloads, ct);

            // 3. アトミックスワップ
            SwapDirectories();
        }
        catch
        {
            SafeDeleteDirectory(_tempDirectory);
            SafeDeleteDirectory(_stagingDirectory);
            throw;
        }
    }

    protected virtual async Task WriteSnapshotAndPayloadsAsync(
        string targetDirectory,
        SessionSnapshot snapshot,
        IReadOnlyDictionary<string, byte[]> imagePayloads,
        CancellationToken ct)
    {
        var jsonPath = Path.Combine(targetDirectory, "session.json");
        var json = JsonSerializer.Serialize(snapshot, _jsonOptions);
        await File.WriteAllTextAsync(jsonPath, json, ct);

        foreach (var (payloadRef, data) in imagePayloads)
        {
            ct.ThrowIfCancellationRequested();
            var payloadPath = Path.Combine(targetDirectory, payloadRef);
            await File.WriteAllBytesAsync(payloadPath, data, ct);
        }
    }

    protected virtual void SwapDirectories()
    {
        // staging ディレクトリの準備
        if (Directory.Exists(_stagingDirectory))
        {
            Directory.Delete(_stagingDirectory, recursive: true);
        }
        Directory.Move(_tempDirectory, _stagingDirectory);

        // 古い backup があれば削除
        if (Directory.Exists(_backupDirectory))
        {
            Directory.Delete(_backupDirectory, recursive: true);
        }

        bool hadCurrent = Directory.Exists(_currentDirectory);
        if (hadCurrent)
        {
            Directory.Move(_currentDirectory, _backupDirectory);
        }

        try
        {
            Directory.Move(_stagingDirectory, _currentDirectory);
        }
        catch
        {
            if (hadCurrent && !Directory.Exists(_currentDirectory) && Directory.Exists(_backupDirectory))
            {
                try
                {
                    Directory.Move(_backupDirectory, _currentDirectory);
                }
                catch
                {
                    // ロールバック失敗時は元の例外を優先
                }
            }
            throw;
        }

        if (Directory.Exists(_backupDirectory))
        {
            SafeDeleteDirectory(_backupDirectory);
        }

        SafeDeleteFile(_cleanExitMarkerPath);
    }

    public virtual async Task<SessionSnapshot?> LoadLatestSnapshotAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var sessionJsonPath = Path.Combine(_currentDirectory, "session.json");
        if (!File.Exists(sessionJsonPath))
        {
            return null;
        }

        try
        {
            using var stream = File.OpenRead(sessionJsonPath);
            return await JsonSerializer.DeserializeAsync<SessionSnapshot>(stream, _jsonOptions, ct);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public virtual async Task<byte[]?> LoadImagePayloadAsync(string payloadRef, CancellationToken ct = default)
    {
        ValidatePayloadRef(payloadRef);
        ct.ThrowIfCancellationRequested();

        var payloadPath = Path.Combine(_currentDirectory, payloadRef);
        if (!File.Exists(payloadPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(payloadPath, ct);
    }

    public virtual Task ClearSessionAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        SafeDeleteDirectory(_currentDirectory);
        SafeDeleteDirectory(_tempDirectory);
        SafeDeleteDirectory(_stagingDirectory);
        SafeDeleteDirectory(_backupDirectory);
        SafeDeleteFile(_cleanExitMarkerPath);
        return Task.CompletedTask;
    }

    public virtual Task<bool> HasActiveSessionAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var sessionJsonPath = Path.Combine(_currentDirectory, "session.json");
        bool hasSession = File.Exists(sessionJsonPath);
        bool hasCleanMarker = File.Exists(_cleanExitMarkerPath);

        return Task.FromResult(hasSession && !hasCleanMarker);
    }

    public virtual async Task MarkCleanExitAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (!Directory.Exists(_baseDirectory))
        {
            Directory.CreateDirectory(_baseDirectory);
        }
        await File.WriteAllTextAsync(_cleanExitMarkerPath, DateTimeOffset.UtcNow.ToString("o"), ct);
    }

    private static void SafeDeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            try
            {
                Directory.Delete(path, recursive: true);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static void SafeDeleteFile(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
