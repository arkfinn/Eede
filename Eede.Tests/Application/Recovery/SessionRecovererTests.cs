#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eede.Application.Recovery;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Recovery;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Pictures;
using NUnit.Framework;

namespace Eede.Tests.Application.Recovery;

[TestFixture]
public class SessionRecovererTests
{
    private InMemorySessionStorage _storage = null!;
    private SkiaSharpPictureCodec _codec = null!;
    private SessionRecoverer _recoverer = null!;

    [SetUp]
    public void SetUp()
    {
        _storage = new InMemorySessionStorage();
        _codec = new SkiaSharpPictureCodec();
        _recoverer = new SessionRecoverer(_storage, _codec);
    }

    private static (SessionSnapshot Snapshot, Dictionary<string, byte[]> Payloads) CreateSampleSnapshotWithPayloads(
        SkiaSharpPictureCodec codec,
        bool includePull = true)
    {
        var sessionId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var pic1 = Picture.CreateEmpty(new PictureSize(16, 16));
        var pic2 = Picture.CreateEmpty(new PictureSize(32, 32));
        var pullPic = Picture.CreateEmpty(new PictureSize(8, 8));

        var pic1Bytes = codec.EncodeToPng(pic1);
        var pic2Bytes = codec.EncodeToPng(pic2);
        var pullPicBytes = codec.EncodeToPng(pullPic);

        var doc1Ref = "doc_1.png";
        var doc2Ref = "doc_2.png";
        var pullRef = "pull_canvas.png";

        var doc1 = new DocumentSnapshot("doc-1", "C:/test/file1.png", true, pic1.Size, 1.0f, doc1Ref);
        var doc2 = new DocumentSnapshot("doc-2", null, false, pic2.Size, 2.0f, doc2Ref);

        PullSnapshot? pull = includePull
            ? new PullSnapshot("doc-1", new PictureArea(new Position(4, 4), pullPic.Size), true, pullRef)
            : null;

        var palette = new PaletteSnapshot(
            new ArgbColor(255, 255, 0, 0),
            0,
            new[] { new ArgbColor(255, 0, 0, 0), new ArgbColor(255, 255, 255, 255) });

        var snapshot = new SessionSnapshot(
            sessionId,
            now,
            "doc-1",
            new[] { doc1, doc2 },
            pull,
            palette);

        var payloads = new Dictionary<string, byte[]>
        {
            [doc1Ref] = pic1Bytes,
            [doc2Ref] = pic2Bytes
        };
        if (includePull)
        {
            payloads[pullRef] = pullPicBytes;
        }

        return (snapshot, payloads);
    }

    [Test]
    public async Task HasPendingRecoveryAsync_WhenNoSession_ReturnsFalse()
    {
        var result = await _recoverer.HasPendingRecoveryAsync();
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasPendingRecoveryAsync_WhenActiveSessionExists_ReturnsTrue()
    {
        var (snapshot, payloads) = CreateSampleSnapshotWithPayloads(_codec);
        _storage.DirectSetSession(snapshot, payloads);

        var result = await _recoverer.HasPendingRecoveryAsync();
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task GetRecoveryMetadataAsync_ReturnsLatestSnapshot()
    {
        var (snapshot, payloads) = CreateSampleSnapshotWithPayloads(_codec);
        _storage.DirectSetSession(snapshot, payloads);

        var metadata = await _recoverer.GetRecoveryMetadataAsync();
        Assert.That(metadata, Is.Not.Null);
        Assert.That(metadata!.SessionId, Is.EqualTo(snapshot.SessionId));
        Assert.That(metadata.ActiveDocumentId, Is.EqualTo("doc-1"));
        Assert.That(metadata.Documents.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task RestoreSessionAsync_ValidSnapshotAndPayloads_RestoresAllDataSuccessfully()
    {
        var (snapshot, payloads) = CreateSampleSnapshotWithPayloads(_codec, includePull: true);
        _storage.DirectSetSession(snapshot, payloads);

        var restored = await _recoverer.RestoreSessionAsync();

        Assert.That(restored, Is.Not.Null);
        Assert.That(restored.HasCorruptedDocuments, Is.False);
        Assert.That(restored.CorruptedDocuments, Is.Empty);
        Assert.That(restored.Documents.Count, Is.EqualTo(2));

        var doc1 = restored.Documents.First(d => d.Snapshot.DocumentId == "doc-1");
        Assert.That(doc1.Picture.Width, Is.EqualTo(16));
        Assert.That(doc1.Picture.Height, Is.EqualTo(16));
        Assert.That(doc1.Snapshot.OriginalFilePath, Is.EqualTo("C:/test/file1.png"));
        Assert.That(doc1.Snapshot.IsEdited, Is.True);

        var doc2 = restored.Documents.First(d => d.Snapshot.DocumentId == "doc-2");
        Assert.That(doc2.Picture.Width, Is.EqualTo(32));
        Assert.That(doc2.Picture.Height, Is.EqualTo(32));
        Assert.That(doc2.Snapshot.OriginalFilePath, Is.Null);
        Assert.That(doc2.Snapshot.IsEdited, Is.False);

        // Pull state
        Assert.That(restored.PullState, Is.Not.Null);
        Assert.That(restored.PullState!.Snapshot.SourceDocumentId, Is.EqualTo("doc-1"));
        Assert.That(restored.PullState.CanvasPicture, Is.Not.Null);
        Assert.That(restored.PullState.CanvasPicture!.Width, Is.EqualTo(8));

        // Palette
        Assert.That(restored.PaletteState.SelectedColor, Is.EqualTo(new ArgbColor(255, 255, 0, 0)));
        Assert.That(restored.PaletteState.PaletteColors.Count, Is.EqualTo(2));
    }

    [Test]
    public void RestoreSessionAsync_NoSessionSnapshot_ThrowsInvalidOperationException()
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _recoverer.RestoreSessionAsync();
        });
    }

    [Test]
    public async Task RestoreSessionAsync_CorruptedPayload_AppliesBestEffortRecovery()
    {
        var (snapshot, payloads) = CreateSampleSnapshotWithPayloads(_codec, includePull: false);

        // 意図的に doc-2 のペイロードを破損バイナリに置き換える
        payloads["doc_2.png"] = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44 };
        _storage.DirectSetSession(snapshot, payloads);

        var restored = await _recoverer.RestoreSessionAsync();

        Assert.That(restored, Is.Not.Null);
        Assert.That(restored.HasCorruptedDocuments, Is.True);
        Assert.That(restored.CorruptedDocuments.Count, Is.EqualTo(1));
        Assert.That(restored.CorruptedDocuments[0].Snapshot.DocumentId, Is.EqualTo("doc-2"));

        // 健全な doc-1 は正常に復元されていること
        Assert.That(restored.Documents.Count, Is.EqualTo(1));
        Assert.That(restored.Documents[0].Snapshot.DocumentId, Is.EqualTo("doc-1"));
        Assert.That(restored.Documents[0].Picture.Width, Is.EqualTo(16));
    }

    [Test]
    public async Task RestoreSessionAsync_MissingPayload_AppliesBestEffortRecovery()
    {
        var (snapshot, payloads) = CreateSampleSnapshotWithPayloads(_codec, includePull: false);

        // doc-2 のペイロードを意図的に削除
        payloads.Remove("doc_2.png");
        _storage.DirectSetSession(snapshot, payloads);

        var restored = await _recoverer.RestoreSessionAsync();

        Assert.That(restored, Is.Not.Null);
        Assert.That(restored.HasCorruptedDocuments, Is.True);
        Assert.That(restored.CorruptedDocuments.Count, Is.EqualTo(1));
        Assert.That(restored.CorruptedDocuments[0].Snapshot.DocumentId, Is.EqualTo("doc-2"));
        Assert.That(restored.CorruptedDocuments[0].ErrorMessage, Does.Contain("not found"));

        Assert.That(restored.Documents.Count, Is.EqualTo(1));
        Assert.That(restored.Documents[0].Snapshot.DocumentId, Is.EqualTo("doc-1"));
    }

    [Test]
    public async Task DiscardSessionAsync_ClearsSessionStorage()
    {
        var (snapshot, payloads) = CreateSampleSnapshotWithPayloads(_codec);
        _storage.DirectSetSession(snapshot, payloads);

        Assert.That(await _recoverer.HasPendingRecoveryAsync(), Is.True);

        await _recoverer.DiscardSessionAsync();

        Assert.That(await _recoverer.HasPendingRecoveryAsync(), Is.False);
        Assert.That(await _storage.LoadLatestSnapshotAsync(), Is.Null);
    }

    [Test]
    public void Constructor_WhenStorageIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SessionRecoverer(null!, _codec));
    }

    [Test]
    public void Constructor_WhenCodecIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SessionRecoverer(_storage, null!));
    }

    [Test]
    public async Task RestoreSessionAsync_WhenPullCanvasPayloadCorrupted_HandlesGracefully()
    {
        var (snapshot, payloads) = CreateSampleSnapshotWithPayloads(_codec, includePull: true);
        // 意図的に pull_canvas.png を不正なバイナリに置き換える
        payloads["pull_canvas.png"] = new byte[] { 0xFF, 0xFF, 0x00 };
        _storage.DirectSetSession(snapshot, payloads);

        var restored = await _recoverer.RestoreSessionAsync();

        Assert.That(restored, Is.Not.Null);
        // PullState 自体は保持されるが、CanvasPicture は安全に null にフォールバックされる
        Assert.That(restored.PullState, Is.Not.Null);
        Assert.That(restored.PullState!.CanvasPicture, Is.Null);
        // ドキュメント群の復元は影響を受けず完了する
        Assert.That(restored.Documents.Count, Is.EqualTo(2));
    }
}

