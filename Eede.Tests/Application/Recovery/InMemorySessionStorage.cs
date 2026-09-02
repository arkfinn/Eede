#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eede.Application.Recovery;
using Eede.Domain.ImageEditing.Recovery;

namespace Eede.Tests.Application.Recovery;

public sealed class InMemorySessionStorage : ISessionStorage
{
    private readonly object _lock = new();
    private SessionSnapshot? _latestSnapshot;
    private readonly Dictionary<string, byte[]> _payloads = new();
    private bool _hasActiveSession;
    private int _saveCount;

    public TimeSpan SimulatedDelay { get; set; } = TimeSpan.Zero;
    public Exception? SimulatedException { get; set; }

    public int SaveCount
    {
        get
        {
            lock (_lock) return _saveCount;
        }
    }

    public SessionSnapshot? LatestSnapshot
    {
        get
        {
            lock (_lock) return _latestSnapshot;
        }
    }

    public IReadOnlyDictionary<string, byte[]> Payloads
    {
        get
        {
            lock (_lock) return new Dictionary<string, byte[]>(_payloads);
        }
    }

    public async Task SaveSnapshotAsync(SessionSnapshot snapshot, IReadOnlyDictionary<string, byte[]> imagePayloads, CancellationToken ct = default)
    {
        if (SimulatedDelay > TimeSpan.Zero)
        {
            await Task.Delay(SimulatedDelay, ct);
        }

        ct.ThrowIfCancellationRequested();

        if (SimulatedException is not null)
        {
            throw SimulatedException;
        }

        lock (_lock)
        {
            _latestSnapshot = snapshot;
            foreach (var (key, value) in imagePayloads)
            {
                _payloads[key] = (byte[])value.Clone();
            }
            _hasActiveSession = true;
            _saveCount++;
        }
    }

    public Task<SessionSnapshot?> LoadLatestSnapshotAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_lock)
        {
            return Task.FromResult(_latestSnapshot);
        }
    }

    public Task<byte[]?> LoadImagePayloadAsync(string payloadRef, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_lock)
        {
            if (_payloads.TryGetValue(payloadRef, out var bytes))
            {
                return Task.FromResult<byte[]?>((byte[])bytes.Clone());
            }
            return Task.FromResult<byte[]?>(null);
        }
    }

    public Task ClearSessionAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_lock)
        {
            _latestSnapshot = null;
            _payloads.Clear();
            _hasActiveSession = false;
            IsCleanExitMarked = false;
            return Task.CompletedTask;
        }
    }

    public Task<bool> HasActiveSessionAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_lock)
        {
            return Task.FromResult(_hasActiveSession);
        }
    }

    public Task<bool> HasCleanExitMarkerAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_lock)
        {
            return Task.FromResult(IsCleanExitMarked);
        }
    }

    public bool IsCleanExitMarked { get; private set; }

    public Task MarkCleanExitAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_lock)
        {
            IsCleanExitMarked = true;
            return Task.CompletedTask;
        }
    }

    public void DirectSetPayload(string payloadRef, byte[] data)
    {
        lock (_lock)
        {
            _payloads[payloadRef] = (byte[])data.Clone();
        }
    }

    public void DirectSetSession(SessionSnapshot snapshot, IReadOnlyDictionary<string, byte[]>? payloads = null)
    {
        lock (_lock)
        {
            _latestSnapshot = snapshot;
            _hasActiveSession = true;
            if (payloads is not null)
            {
                foreach (var (key, value) in payloads)
                {
                    _payloads[key] = (byte[])value.Clone();
                }
            }
        }
    }
}
