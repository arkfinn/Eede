#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Eede.Domain.ImageEditing.Recovery;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Recovery;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Recovery;

[TestFixture]
public class BrowserIndexedDbSessionStorageTests
{
    private string? _storedSnapshotJson;
    private Dictionary<string, string> _storedPayloads = new();
    private bool _storedCleanExit;
    private bool _failSave;

    private BrowserIndexedDbSessionStorage CreateStorage()
    {
        _storedSnapshotJson = null;
        _storedPayloads.Clear();
        _storedCleanExit = false;
        _failSave = false;

        return new BrowserIndexedDbSessionStorage(
            customSave: (snapshotJson, payloadsJson) =>
            {
                if (_failSave)
                {
                    throw new InvalidOperationException("QuotaExceededError: Storage quota exceeded.");
                }
                _storedSnapshotJson = snapshotJson;
                _storedPayloads = JsonSerializer.Deserialize<Dictionary<string, string>>(payloadsJson) ?? new();
                _storedCleanExit = false;
                return Task.FromResult(true);
            },
            customLoadLatest: () => Task.FromResult(_storedSnapshotJson),
            customLoadPayload: (key) =>
            {
                _storedPayloads.TryGetValue(key, out var val);
                return Task.FromResult(val);
            },
            customClear: () =>
            {
                _storedSnapshotJson = null;
                _storedPayloads.Clear();
                return Task.FromResult(true);
            },
            customHasActive: () => Task.FromResult(_storedSnapshotJson != null),
            customMarkCleanExit: () =>
            {
                _storedCleanExit = true;
                return Task.FromResult(true);
            },
            customHasCleanExit: () => Task.FromResult(_storedCleanExit)
        );
    }

    private static SessionSnapshot CreateDummySnapshot(string docId = "doc-1", string? payloadRef = "payload-1")
    {
        var doc = new DocumentSnapshot(docId, "hero.png", true, new PictureSize(16, 16), 1.0f, payloadRef);
        var tab = new PaletteTabSnapshot("hero.png", true, new ArgbColor[256]);
        var palette = new PaletteSnapshot(new ArgbColor(255, 255, 0, 0), 0, new ArgbColor[256], new[] { tab });

        return new SessionSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            docId,
            new[] { doc },
            null,
            palette);
    }

    [Test]
    public async Task SaveSnapshotAsync_And_LoadLatestSnapshotAsync_RestoresIdenticalSnapshotAndPayloads()
    {
        var storage = CreateStorage();
        var snapshot = CreateDummySnapshot();
        byte[] payloadBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3, 4 };
        var payloads = new Dictionary<string, byte[]> { ["payload-1"] = payloadBytes };

        await storage.SaveSnapshotAsync(snapshot, payloads);

        Assert.That(await storage.HasActiveSessionAsync(), Is.True);

        var restored = await storage.LoadLatestSnapshotAsync();
        Assert.That(restored, Is.Not.Null);
        Assert.That(restored!.SessionId, Is.EqualTo(snapshot.SessionId));
        Assert.That(restored.ActiveDocumentId, Is.EqualTo("doc-1"));
        Assert.That(restored.Documents.Count, Is.EqualTo(1));
        Assert.That(restored.Documents[0].OriginalFilePath, Is.EqualTo("hero.png"));

        var restoredPayload = await storage.LoadImagePayloadAsync("payload-1");
        Assert.That(restoredPayload, Is.EqualTo(payloadBytes));
    }

    [Test]
    public void SaveSnapshotAsync_WhenQuotaExceeded_GracefullyDegradesWithoutCrashing()
    {
        var storage = CreateStorage();
        _failSave = true; // 容量オーバー（QuotaExceededError）をシミュレート
        var snapshot = CreateDummySnapshot();
        var payloads = new Dictionary<string, byte[]> { ["payload-1"] = new byte[] { 1, 2, 3 } };

        // 例外が外へ漏れず、アプリがクラッシュしないこと！
        Assert.DoesNotThrowAsync(async () => await storage.SaveSnapshotAsync(snapshot, payloads));
    }

    [Test]
    public async Task LoadLatestSnapshotAsync_WhenStorageEmpty_ReturnsNullWithoutException()
    {
        var storage = CreateStorage();

        Assert.That(await storage.HasActiveSessionAsync(), Is.False);
        Assert.That(await storage.LoadLatestSnapshotAsync(), Is.Null);
        Assert.That(await storage.LoadImagePayloadAsync("non-existent"), Is.Null);
    }

    [Test]
    public async Task CleanExitMarker_TracksStateCorrectly()
    {
        var storage = CreateStorage();

        Assert.That(await storage.HasCleanExitMarkerAsync(), Is.False);

        await storage.MarkCleanExitAsync();
        Assert.That(await storage.HasCleanExitMarkerAsync(), Is.True);

        // 新規スナップショット保存で clean exit マーカーがリセットされること
        await storage.SaveSnapshotAsync(CreateDummySnapshot(), new Dictionary<string, byte[]>());
        Assert.That(await storage.HasCleanExitMarkerAsync(), Is.False);
    }

    [Test]
    public async Task ClearSessionAsync_RemovesAllData()
    {
        var storage = CreateStorage();
        await storage.SaveSnapshotAsync(CreateDummySnapshot(), new Dictionary<string, byte[]> { ["p1"] = new byte[] { 1 } });

        Assert.That(await storage.HasActiveSessionAsync(), Is.True);

        await storage.ClearSessionAsync();

        Assert.That(await storage.HasActiveSessionAsync(), Is.False);
        Assert.That(await storage.LoadLatestSnapshotAsync(), Is.Null);
        Assert.That(await storage.LoadImagePayloadAsync("p1"), Is.Null);
    }
}
