#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eede.Domain.ImageEditing.Recovery;

namespace Eede.Application.Recovery;

public interface ISessionStorage
{
    Task SaveSnapshotAsync(SessionSnapshot snapshot, IReadOnlyDictionary<string, byte[]> imagePayloads, CancellationToken ct = default);
    Task<SessionSnapshot?> LoadLatestSnapshotAsync(CancellationToken ct = default);
    Task<byte[]?> LoadImagePayloadAsync(string payloadRef, CancellationToken ct = default);
    Task ClearSessionAsync(CancellationToken ct = default);
    Task<bool> HasActiveSessionAsync(CancellationToken ct = default);
    Task<bool> HasCleanExitMarkerAsync(CancellationToken ct = default);
    Task MarkCleanExitAsync(CancellationToken ct = default);
}
