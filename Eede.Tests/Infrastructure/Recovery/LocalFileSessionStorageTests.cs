#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eede.Domain.ImageEditing.Recovery;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Recovery;
using NUnit.Framework;

namespace Eede.Tests.Infrastructure.Recovery;

[TestFixture]
public class LocalFileSessionStorageTests
{
    private string _testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "EedeRecoveryTests_" + Guid.NewGuid().ToString("N"));
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch
            {
                // ベストエフォート
            }
        }
    }

    private static SessionSnapshot CreateSampleSnapshot(Guid? sessionId = null, bool includePull = true)
    {
        var docs = new[]
        {
            new DocumentSnapshot("doc-1", @"C:\path\file1.png", true, new PictureSize(32, 32), 2.0f, "payload_doc1.bin"),
            new DocumentSnapshot("doc-2", null, false, new PictureSize(64, 48), 1.0f, "payload_doc2.bin")
        };

        PullSnapshot? pull = includePull
            ? new PullSnapshot("doc-1", new PictureArea(new Position(4, 8), new PictureSize(16, 16)), true, "payload_canvas.bin")
            : null;

        var palette = new PaletteSnapshot(
            new ArgbColor(255, 128, 64, 32),
            1,
            new[] { new ArgbColor(255, 0, 0, 0), new ArgbColor(255, 255, 255, 255) });

        return new SessionSnapshot(
            sessionId ?? Guid.NewGuid(),
            new DateTimeOffset(2026, 9, 2, 12, 0, 0, TimeSpan.Zero),
            "doc-1",
            docs,
            pull,
            palette);
    }

    [Test]
    public async Task RoundTrip_WithPullStateAndMultiplePayloads_RestoresAllDataCorrectly()
    {
        var storage = new LocalFileSessionStorage(_testDirectory);
        var snapshot = CreateSampleSnapshot(includePull: true);
        var payloads = new Dictionary<string, byte[]>
        {
            ["payload_doc1.bin"] = new byte[] { 1, 2, 3, 4 },
            ["payload_doc2.bin"] = new byte[] { 5, 6, 7, 8 },
            ["payload_canvas.bin"] = new byte[] { 9, 10, 11, 12 }
        };

        await storage.SaveSnapshotAsync(snapshot, payloads);

        var loadedSnapshot = await storage.LoadLatestSnapshotAsync();
        Assert.That(loadedSnapshot, Is.Not.Null);
        Assert.That(loadedSnapshot!.SessionId, Is.EqualTo(snapshot.SessionId));
        Assert.That(loadedSnapshot.CreatedAt, Is.EqualTo(snapshot.CreatedAt));
        Assert.That(loadedSnapshot.ActiveDocumentId, Is.EqualTo(snapshot.ActiveDocumentId));

        Assert.That(loadedSnapshot.Documents.Count, Is.EqualTo(2));
        Assert.That(loadedSnapshot.Documents[0].DocumentId, Is.EqualTo("doc-1"));
        Assert.That(loadedSnapshot.Documents[0].OriginalFilePath, Is.EqualTo(@"C:\path\file1.png"));
        Assert.That(loadedSnapshot.Documents[0].IsEdited, Is.True);
        Assert.That(loadedSnapshot.Documents[0].Size, Is.EqualTo(new PictureSize(32, 32)));
        Assert.That(loadedSnapshot.Documents[0].Magnification, Is.EqualTo(2.0f));
        Assert.That(loadedSnapshot.Documents[0].ImagePayloadRef, Is.EqualTo("payload_doc1.bin"));

        Assert.That(loadedSnapshot.Documents[1].DocumentId, Is.EqualTo("doc-2"));
        Assert.That(loadedSnapshot.Documents[1].OriginalFilePath, Is.Null);
        Assert.That(loadedSnapshot.Documents[1].IsEdited, Is.False);
        Assert.That(loadedSnapshot.Documents[1].Size, Is.EqualTo(new PictureSize(64, 48)));
        Assert.That(loadedSnapshot.Documents[1].Magnification, Is.EqualTo(1.0f));
        Assert.That(loadedSnapshot.Documents[1].ImagePayloadRef, Is.EqualTo("payload_doc2.bin"));

        Assert.That(loadedSnapshot.PullState, Is.Not.Null);
        Assert.That(loadedSnapshot.PullState!.SourceDocumentId, Is.EqualTo("doc-1"));
        Assert.That(loadedSnapshot.PullState.SourceArea.Position, Is.EqualTo(new Position(4, 8)));
        Assert.That(loadedSnapshot.PullState.SourceArea.Size, Is.EqualTo(new PictureSize(16, 16)));
        Assert.That(loadedSnapshot.PullState.HasUnpushedChanges, Is.True);
        Assert.That(loadedSnapshot.PullState.CanvasImagePayloadRef, Is.EqualTo("payload_canvas.bin"));

        Assert.That(loadedSnapshot.PaletteState.SelectedColor, Is.EqualTo(new ArgbColor(255, 128, 64, 32)));
        Assert.That(loadedSnapshot.PaletteState.ActiveTabIndex, Is.EqualTo(1));
        Assert.That(loadedSnapshot.PaletteState.PaletteColors.Count, Is.EqualTo(2));
        Assert.That(loadedSnapshot.PaletteState.PaletteColors[0], Is.EqualTo(new ArgbColor(255, 0, 0, 0)));
        Assert.That(loadedSnapshot.PaletteState.PaletteColors[1], Is.EqualTo(new ArgbColor(255, 255, 255, 255)));

        var payload1 = await storage.LoadImagePayloadAsync("payload_doc1.bin");
        var payload2 = await storage.LoadImagePayloadAsync("payload_doc2.bin");
        var payloadCanvas = await storage.LoadImagePayloadAsync("payload_canvas.bin");

        Assert.That(payload1, Is.EqualTo(new byte[] { 1, 2, 3, 4 }));
        Assert.That(payload2, Is.EqualTo(new byte[] { 5, 6, 7, 8 }));
        Assert.That(payloadCanvas, Is.EqualTo(new byte[] { 9, 10, 11, 12 }));
    }

    [Test]
    public async Task RoundTrip_WithoutPullState_RestoresNullPullState()
    {
        var storage = new LocalFileSessionStorage(_testDirectory);
        var snapshot = CreateSampleSnapshot(includePull: false);
        var payloads = new Dictionary<string, byte[]>
        {
            ["payload_doc1.bin"] = new byte[] { 1 }
        };

        await storage.SaveSnapshotAsync(snapshot, payloads);

        var loaded = await storage.LoadLatestSnapshotAsync();
        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.PullState, Is.Null);
    }

    [TestCase("../../test.png")]
    [TestCase("../session.json")]
    [TestCase("dir/sub.png")]
    [TestCase("dir\\sub.png")]
    [TestCase("test*name.bin")]
    [TestCase("test?name.bin")]
    [TestCase("..")]
    [TestCase("")]
    [TestCase("   ")]
    public void PathTraversal_ThrowsArgumentException_OnSave(string invalidRef)
    {
        var storage = new LocalFileSessionStorage(_testDirectory);
        var snapshot = CreateSampleSnapshot();
        var payloads = new Dictionary<string, byte[]>
        {
            [invalidRef] = new byte[] { 1, 2, 3 }
        };

        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await storage.SaveSnapshotAsync(snapshot, payloads);
        });
    }

    [TestCase("../../test.png")]
    [TestCase("../session.json")]
    [TestCase("dir/sub.png")]
    [TestCase("dir\\sub.png")]
    [TestCase("test*name.bin")]
    [TestCase("test?name.bin")]
    [TestCase("..")]
    [TestCase("")]
    [TestCase("   ")]
    public void PathTraversal_ThrowsArgumentException_OnLoad(string invalidRef)
    {
        var storage = new LocalFileSessionStorage(_testDirectory);
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await storage.LoadImagePayloadAsync(invalidRef);
        });
    }

    [Test]
    public async Task AtomicSave_WhenSecondSaveFailsDuringWrite_KeepsFirstSaveIntact()
    {
        var failingStorage = new FailingWriteSessionStorage(_testDirectory);
        var firstSnapshot = CreateSampleSnapshot(Guid.NewGuid());
        var firstPayloads = new Dictionary<string, byte[]>
        {
            ["payload_doc1.bin"] = new byte[] { 10, 20, 30 }
        };

        // 1回目の保存は成功する
        await failingStorage.SaveSnapshotAsync(firstSnapshot, firstPayloads);

        var initialLoaded = await failingStorage.LoadLatestSnapshotAsync();
        Assert.That(initialLoaded, Is.Not.Null);
        Assert.That(initialLoaded!.SessionId, Is.EqualTo(firstSnapshot.SessionId));

        // 2回目の保存で書き込み途中に例外を発生させる
        failingStorage.ShouldFail = true;
        var secondSnapshot = CreateSampleSnapshot(Guid.NewGuid());
        var secondPayloads = new Dictionary<string, byte[]>
        {
            ["payload_doc1.bin"] = new byte[] { 99, 99, 99 }
        };

        Assert.ThrowsAsync<IOException>(async () =>
        {
            await failingStorage.SaveSnapshotAsync(secondSnapshot, secondPayloads);
        });

        // 1回目の正常なデータが維持されていること
        var afterFailedLoaded = await failingStorage.LoadLatestSnapshotAsync();
        Assert.That(afterFailedLoaded, Is.Not.Null);
        Assert.That(afterFailedLoaded!.SessionId, Is.EqualTo(firstSnapshot.SessionId));

        var payload = await failingStorage.LoadImagePayloadAsync("payload_doc1.bin");
        Assert.That(payload, Is.EqualTo(new byte[] { 10, 20, 30 }));
    }

    [Test]
    public async Task CleanExitLifecycle_TransitionsStateCorrectly()
    {
        var storage = new LocalFileSessionStorage(_testDirectory);

        // 初期状態: セッションなし
        Assert.That(await storage.HasActiveSessionAsync(), Is.False);

        // スナップショット保存直後: アクティブセッションあり
        var snapshot1 = CreateSampleSnapshot();
        await storage.SaveSnapshotAsync(snapshot1, new Dictionary<string, byte[]>());
        Assert.That(await storage.HasActiveSessionAsync(), Is.True);

        // クリーン終了マーク後: アクティブセッションなし
        await storage.MarkCleanExitAsync();
        Assert.That(await storage.HasActiveSessionAsync(), Is.False);

        // 新しいセッション保存: clean_exit.marker が解除され再びアクティブに
        var snapshot2 = CreateSampleSnapshot();
        await storage.SaveSnapshotAsync(snapshot2, new Dictionary<string, byte[]>());
        Assert.That(await storage.HasActiveSessionAsync(), Is.True);
    }

    [Test]
    public async Task ClearSession_DeletesAllSessionData()
    {
        var storage = new LocalFileSessionStorage(_testDirectory);
        var snapshot = CreateSampleSnapshot();
        var payloads = new Dictionary<string, byte[]>
        {
            ["payload_doc1.bin"] = new byte[] { 1, 2, 3 }
        };

        await storage.SaveSnapshotAsync(snapshot, payloads);
        Assert.That(await storage.HasActiveSessionAsync(), Is.True);

        await storage.ClearSessionAsync();

        Assert.That(await storage.HasActiveSessionAsync(), Is.False);
        Assert.That(await storage.LoadLatestSnapshotAsync(), Is.Null);
        Assert.That(await storage.LoadImagePayloadAsync("payload_doc1.bin"), Is.Null);
        Assert.That(Directory.Exists(Path.Combine(_testDirectory, "current")), Is.False);
    }

    [Test]
    public async Task LoadImagePayloadAsync_WhenFileDoesNotExist_ReturnsNull()
    {
        var storage = new LocalFileSessionStorage(_testDirectory);
        var snapshot = CreateSampleSnapshot();
        await storage.SaveSnapshotAsync(snapshot, new Dictionary<string, byte[]>());

        var result = await storage.LoadImagePayloadAsync("nonexistent.bin");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task LoadLatestSnapshotAsync_WhenNoSessionExists_ReturnsNull()
    {
        var storage = new LocalFileSessionStorage(_testDirectory);
        var result = await storage.LoadLatestSnapshotAsync();
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task LoadLatestSnapshotAsync_WhenJsonIsCorrupted_ReturnsNull()
    {
        var storage = new LocalFileSessionStorage(_testDirectory);
        var currentDir = Path.Combine(_testDirectory, "current");
        Directory.CreateDirectory(currentDir);
        await File.WriteAllTextAsync(Path.Combine(currentDir, "session.json"), "{ invalid json syntax }}}");

        var result = await storage.LoadLatestSnapshotAsync();
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Constructor_WithInvalidBaseDirectory_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentNullException>(() => new LocalFileSessionStorage(null!));
        Assert.Throws<ArgumentException>(() => new LocalFileSessionStorage(""));
        Assert.Throws<ArgumentException>(() => new LocalFileSessionStorage("   "));
    }

    [Test]
    public void SaveSnapshotAsync_WithNullArguments_ThrowsArgumentNullException()
    {
        var storage = new LocalFileSessionStorage(_testDirectory);
        var snapshot = CreateSampleSnapshot();

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await storage.SaveSnapshotAsync(null!, new Dictionary<string, byte[]>());
        });

        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await storage.SaveSnapshotAsync(snapshot, null!);
        });
    }

    [Test]
    public void DefaultConstructor_InitializesToDefaultDirectory()
    {
        var storage = new LocalFileSessionStorage();
        Assert.That(storage.BaseDirectory, Is.Not.Null.And.Not.Empty);
        Assert.That(storage.BaseDirectory, Does.Contain("recovery"));
    }

    private class FailingWriteSessionStorage : LocalFileSessionStorage
    {
        public bool ShouldFail { get; set; }

        public FailingWriteSessionStorage(string baseDirectory) : base(baseDirectory) { }

        protected override Task WriteSnapshotAndPayloadsAsync(
            string targetDirectory,
            SessionSnapshot snapshot,
            IReadOnlyDictionary<string, byte[]> imagePayloads,
            CancellationToken ct)
        {
            if (ShouldFail)
            {
                throw new IOException("Simulated disk failure during write.");
            }
            return base.WriteSnapshotAndPayloadsAsync(targetDirectory, snapshot, imagePayloads, ct);
        }
    }
}
