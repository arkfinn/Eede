#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Recovery;

namespace Eede.Application.Recovery;

public sealed class SessionRecoveryService : ISessionRecoveryService
{
    private readonly ISessionStorage _storage;
    private readonly IPictureCodec _codec;

    public SessionRecoveryService(ISessionStorage storage, IPictureCodec codec)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _codec = codec ?? throw new ArgumentNullException(nameof(codec));
    }

    public async Task<bool> HasPendingRecoveryAsync(CancellationToken ct = default)
    {
        return await _storage.HasActiveSessionAsync(ct);
    }

    public async Task<SessionSnapshot?> GetRecoveryMetadataAsync(CancellationToken ct = default)
    {
        return await _storage.LoadLatestSnapshotAsync(ct);
    }

    public async Task DiscardSessionAsync(CancellationToken ct = default)
    {
        await _storage.ClearSessionAsync(ct);
    }

    public async Task<RestoredSessionData> RestoreSessionAsync(CancellationToken ct = default)
    {
        var snapshot = await _storage.LoadLatestSnapshotAsync(ct);
        if (snapshot is null)
        {
            throw new InvalidOperationException("No recovery session snapshot found.");
        }

        var restoredDocs = new List<RestoredDocument>();
        var corruptedDocs = new List<CorruptedDocumentInfo>();

        foreach (var docSnapshot in snapshot.Documents)
        {
            ct.ThrowIfCancellationRequested();

            if (docSnapshot.ImagePayloadRef is null)
            {
                var emptyPicture = Picture.CreateEmpty(docSnapshot.Size);
                restoredDocs.Add(new RestoredDocument(docSnapshot, emptyPicture));
                continue;
            }

            try
            {
                var payload = await _storage.LoadImagePayloadAsync(docSnapshot.ImagePayloadRef, ct);
                if (payload is null || payload.Length == 0)
                {
                    corruptedDocs.Add(new CorruptedDocumentInfo(
                        docSnapshot,
                        $"Image payload '{docSnapshot.ImagePayloadRef}' was not found."));
                    continue;
                }

                var picture = _codec.DecodeFromPng(payload);
                restoredDocs.Add(new RestoredDocument(docSnapshot, picture));
            }
            catch (Exception ex)
            {
                corruptedDocs.Add(new CorruptedDocumentInfo(
                    docSnapshot,
                    $"Failed to decode payload '{docSnapshot.ImagePayloadRef}': {ex.Message}",
                    ex));
            }
        }

        RestoredPullState? restoredPull = null;
        if (snapshot.PullState is not null)
        {
            Picture? canvasPicture = null;
            if (snapshot.PullState.CanvasImagePayloadRef is not null)
            {
                try
                {
                    var payload = await _storage.LoadImagePayloadAsync(snapshot.PullState.CanvasImagePayloadRef, ct);
                    if (payload is not null && payload.Length > 0)
                    {
                        canvasPicture = _codec.DecodeFromPng(payload);
                    }
                }
                catch (Exception)
                {
                    // Pull canvas image corruption is isolated to avoid breaking the entire session restore
                }
            }

            restoredPull = new RestoredPullState(snapshot.PullState, canvasPicture);
        }

        return new RestoredSessionData(
            snapshot,
            restoredDocs,
            restoredPull,
            snapshot.PaletteState,
            corruptedDocs);
    }
}
