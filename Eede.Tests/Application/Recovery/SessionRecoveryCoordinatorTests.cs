#nullable enable
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Eede.Application.Recovery;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Recovery;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Pictures;
using Microsoft.Reactive.Testing;
using NUnit.Framework;

namespace Eede.Tests.Application.Recovery;

[TestFixture]
public class SessionRecoveryCoordinatorTests
{
    private InMemorySessionStorage _storage = null!;
    private SkiaSharpPictureCodec _codec = null!;
    private TestScheduler _scheduler = null!;

    [SetUp]
    public void SetUp()
    {
        _storage = new InMemorySessionStorage();
        _codec = new SkiaSharpPictureCodec();
        _scheduler = new TestScheduler();
    }

    private static SessionCapture CreateCapture(string documentId, int width = 16, int height = 16)
    {
        var pic = Picture.CreateEmpty(new PictureSize(width, height));
        var payloadRef = $"doc_{documentId}.png";
        var doc = new DocumentSnapshot(documentId, null, true, pic.Size, 1.0f, payloadRef);
        var palette = new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, Array.Empty<ArgbColor>());

        var snapshot = new SessionSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            documentId,
            new[] { doc },
            null,
            palette);

        var dict = new Dictionary<string, Picture>
        {
            [payloadRef] = pic
        };

        return new SessionCapture(snapshot, dict);
    }

    [Test]
    public async Task Debounce_MultipleRapidEvents_SavesOnlyLatestState()
    {
        int captureCounter = 0;
        SessionCapture currentCapture = CreateCapture("doc-v0");

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            captureFactory: () =>
            {
                Interlocked.Increment(ref captureCounter);
                return currentCapture;
            },
            dirtyStream: null,
            debounceDuration: TimeSpan.FromMilliseconds(500),
            scheduler: _scheduler);

        var savedTaskCompletionSource = new TaskCompletionSource<SessionSnapshot>();
        using var sub = coordinator.SnapshotSaved.Subscribe(s => savedTaskCompletionSource.TrySetResult(s));

        // 1回目のダーティ通知
        currentCapture = CreateCapture("doc-v1");
        coordinator.NotifyDirty();
        _scheduler.AdvanceBy(TimeSpan.FromMilliseconds(200).Ticks);

        Assert.That(_storage.SaveCount, Is.EqualTo(0), "Should not save before debounce time");

        // 2回目のダーティ通知 (タイマーリセット)
        currentCapture = CreateCapture("doc-v2");
        coordinator.NotifyDirty();
        _scheduler.AdvanceBy(TimeSpan.FromMilliseconds(200).Ticks);

        Assert.That(_storage.SaveCount, Is.EqualTo(0), "Should not save before debounce time");

        // 3回目のダーティ通知 (最新)
        currentCapture = CreateCapture("doc-v3");
        coordinator.NotifyDirty();

        // デバウンス時間を経過させる (500ms進める)
        _scheduler.AdvanceBy(TimeSpan.FromMilliseconds(550).Ticks);

        // バックグラウンド保存の完了を待機
        var savedSnapshot = await savedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(3));

        Assert.That(_storage.SaveCount, Is.EqualTo(1));
        Assert.That(_storage.LatestSnapshot, Is.Not.Null);
        Assert.That(_storage.LatestSnapshot!.ActiveDocumentId, Is.EqualTo("doc-v3"));
        Assert.That(savedSnapshot.ActiveDocumentId, Is.EqualTo("doc-v3"));
    }

    [Test]
    public async Task FlushAsync_ImmediateSave_SavesWithoutWaitingForDebounce()
    {
        var capture = CreateCapture("doc-flush");

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            captureFactory: () => capture,
            debounceDuration: TimeSpan.FromSeconds(10), // 長いデバウンス
            scheduler: _scheduler);

        Assert.That(_storage.SaveCount, Is.EqualTo(0));

        // 即時フラッシュ実行
        await coordinator.FlushAsync();

        Assert.That(_storage.SaveCount, Is.EqualTo(1));
        Assert.That(_storage.LatestSnapshot, Is.Not.Null);
        Assert.That(_storage.LatestSnapshot!.ActiveDocumentId, Is.EqualTo("doc-flush"));
    }

    [Test]
    public async Task FlushAsync_WithDirectCapture_SavesSpecifiedCapture()
    {
        var defaultCapture = CreateCapture("doc-default");
        var directCapture = CreateCapture("doc-direct");

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            captureFactory: () => defaultCapture,
            scheduler: _scheduler);

        await coordinator.FlushAsync(directCapture);

        Assert.That(_storage.SaveCount, Is.EqualTo(1));
        Assert.That(_storage.LatestSnapshot!.ActiveDocumentId, Is.EqualTo("doc-direct"));
    }

    [Test]
    public async Task CancellationAndSerialization_ConcurrentSaves_SerializesAndCancelsPriorSave()
    {
        // ストレージ書き込みに 150ms の遅延をシミュレート
        _storage.SimulatedDelay = TimeSpan.FromMilliseconds(150);

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            captureFactory: () => CreateCapture("doc-unused"),
            scheduler: _scheduler);

        var capture1 = CreateCapture("doc-first");
        var capture2 = CreateCapture("doc-second");

        // 1つ目の保存を開始 (非同期で遅延中)
        var task1 = coordinator.FlushAsync(capture1);

        // 少しだけ待機してタスク1が実行中であることを担保
        await Task.Delay(20);

        // 2つ目の保存を直ちに開始 (タスク1にキャンセル要求が送られる)
        var task2 = coordinator.FlushAsync(capture2);

        // 両タスクを待機 (task1はキャンセル中断、task2は正常完了)
        await Task.WhenAll(task1, task2);

        // 最終的なストレージ状態は2つ目のスナップショットであること
        Assert.That(_storage.LatestSnapshot, Is.Not.Null);
        Assert.That(_storage.LatestSnapshot!.ActiveDocumentId, Is.EqualTo("doc-second"));
    }

    [Test]
    public void Dispose_ThrowsObjectDisposedExceptionOnSubsequentCalls()
    {
        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            captureFactory: () => CreateCapture("doc-1"),
            scheduler: _scheduler);

        coordinator.Dispose();

        Assert.Throws<ObjectDisposedException>(() => coordinator.NotifyDirty());
        Assert.ThrowsAsync<ObjectDisposedException>(async () => await coordinator.FlushAsync());
    }

    [Test]
    public void ExternalDirtyStream_TriggersDebouncedSave()
    {
        var dirtySubject = new Subject<string>();
        var capture = CreateCapture("doc-stream");

        using var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            captureFactory: () => capture,
            dirtyStream: dirtySubject,
            debounceDuration: TimeSpan.FromMilliseconds(300),
            scheduler: _scheduler);

        dirtySubject.OnNext("change-1");
        _scheduler.AdvanceBy(TimeSpan.FromMilliseconds(100).Ticks);
        Assert.That(_storage.SaveCount, Is.EqualTo(0));

        _scheduler.AdvanceBy(TimeSpan.FromMilliseconds(250).Ticks);

        // 保存タスクの完了を待機
        if (coordinator.LastSaveTask is not null)
        {
            coordinator.LastSaveTask.GetAwaiter().GetResult();
        }

        Assert.That(_storage.SaveCount, Is.EqualTo(1));
        Assert.That(_storage.LatestSnapshot!.ActiveDocumentId, Is.EqualTo("doc-stream"));
    }

    [Test]
    public async Task FlushAsync_SequentialCalls_CompletesSuccessfullyWithoutDisposedException()
    {
        // 1回目の保存が完了した後に、2回目の保存が正常に行われること（CTS破棄後再利用バグの回帰テスト）
        var capture1 = CreateCapture("doc-first");
        var capture2 = CreateCapture("doc-second");

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            captureFactory: () => capture1,
            scheduler: _scheduler);

        await coordinator.FlushAsync(capture1);
        Assert.That(_storage.SaveCount, Is.EqualTo(1));
        Assert.That(_storage.LatestSnapshot!.ActiveDocumentId, Is.EqualTo("doc-first"));

        // 1回目完了後、2回目の FlushAsync が ObjectDisposedException を投げずに正常に完了すること
        Assert.DoesNotThrowAsync(async () => await coordinator.FlushAsync(capture2));
        Assert.That(_storage.SaveCount, Is.EqualTo(2));
        Assert.That(_storage.LatestSnapshot!.ActiveDocumentId, Is.EqualTo("doc-second"));
    }

    [Test]
    public async Task FlushAsync_MultiplePictures_EncodesAndSavesAllPicturesSuccessfully()
    {
        var dict = new Dictionary<string, Picture>();
        var docs = new List<DocumentSnapshot>();

        for (int i = 0; i < 4; i++)
        {
            var id = $"doc_{i}";
            var pic = Picture.CreateEmpty(new PictureSize(16, 16));
            var payloadRef = $"payload_{i}.png";
            dict[payloadRef] = pic;
            docs.Add(new DocumentSnapshot(id, null, true, pic.Size, 1.0f, payloadRef));
        }

        var palette = new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, Array.Empty<ArgbColor>());
        var snapshot = new SessionSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "doc_0",
            docs,
            null,
            palette);

        var capture = new SessionCapture(snapshot, dict);

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            captureFactory: () => capture,
            scheduler: _scheduler,
            maxParallelism: 2);

        await coordinator.FlushAsync();

        Assert.That(_storage.SaveCount, Is.EqualTo(1));
        Assert.That(_storage.LatestSnapshot, Is.Not.Null);

        foreach (var key in dict.Keys)
        {
            var payload = await _storage.LoadImagePayloadAsync(key);
            Assert.That(payload, Is.Not.Null, $"Payload for key '{key}' should be saved in storage.");
            Assert.That(payload!.Length, Is.GreaterThan(0));
        }
    }

    private class CancelingPictureCodec : Eede.Application.Pictures.IPictureCodec
    {
        private readonly Eede.Application.Pictures.IPictureCodec _inner;
        private readonly CancellationTokenSource _cts;

        public CancelingPictureCodec(Eede.Application.Pictures.IPictureCodec inner, CancellationTokenSource cts)
        {
            _inner = inner;
            _cts = cts;
        }

        public Picture DecodeFromPng(byte[] bytes) => _inner.DecodeFromPng(bytes);

        public byte[] EncodeToPng(Picture picture)
        {
            _cts.Cancel();
            return _inner.EncodeToPng(picture);
        }
    }

    [Test]
    public async Task FlushAsync_ExternalCancellation_ThrowsOperationCanceledExceptionAndDoesNotReportError()
    {
        var dict = new Dictionary<string, Picture>();
        var docs = new List<DocumentSnapshot>();

        for (int i = 0; i < 8; i++)
        {
            var id = $"doc_{i}";
            var pic = Picture.CreateEmpty(new PictureSize(16, 16));
            var payloadRef = $"payload_{i}.png";
            dict[payloadRef] = pic;
            docs.Add(new DocumentSnapshot(id, null, true, pic.Size, 1.0f, payloadRef));
        }

        var palette = new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, Array.Empty<ArgbColor>());
        var snapshot = new SessionSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "doc_0",
            docs,
            null,
            palette);

        var capture = new SessionCapture(snapshot, dict);

        using var cts = new CancellationTokenSource();
        var cancelingCodec = new CancelingPictureCodec(_codec, cts);
        var reportedErrors = new List<Exception>();

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            cancelingCodec,
            captureFactory: () => capture,
            scheduler: _scheduler,
            maxParallelism: 2);

        using var sub = coordinator.SaveErrors.Subscribe(ex => reportedErrors.Add(ex));

        Assert.ThrowsAsync<OperationCanceledException>(async () => await coordinator.FlushAsync(capture, cts.Token));

        Assert.That(reportedErrors, Is.Empty, "Cancellation during parallel encoding should not be reported to SaveErrors.");
        Assert.That(_storage.SaveCount, Is.EqualTo(0), "Storage should not be updated when parallel save is canceled.");
    }

    private class FailingPictureCodec : Eede.Application.Pictures.IPictureCodec
    {
        public Picture DecodeFromPng(byte[] bytes) => throw new NotImplementedException();

        public byte[] EncodeToPng(Picture picture)
        {
            throw new InvalidOperationException("Codec encoding failed.");
        }
    }

    [Test]
    public async Task FlushAsync_CodecException_PropagatesToSaveErrorsAndThrows()
    {
        var dict = new Dictionary<string, Picture>();
        var docs = new List<DocumentSnapshot>();

        for (int i = 0; i < 4; i++)
        {
            var id = $"doc_{i}";
            var pic = Picture.CreateEmpty(new PictureSize(16, 16));
            var payloadRef = $"payload_{i}.png";
            dict[payloadRef] = pic;
            docs.Add(new DocumentSnapshot(id, null, true, pic.Size, 1.0f, payloadRef));
        }

        var palette = new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, Array.Empty<ArgbColor>());
        var snapshot = new SessionSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "doc_0",
            docs,
            null,
            palette);

        var capture = new SessionCapture(snapshot, dict);

        var reportedErrors = new List<Exception>();
        var failingCodec = new FailingPictureCodec();

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            failingCodec,
            captureFactory: () => capture,
            scheduler: _scheduler,
            maxParallelism: 2);

        using var sub = coordinator.SaveErrors.Subscribe(ex => reportedErrors.Add(ex));

        var ex = Assert.CatchAsync<Exception>(async () => await coordinator.FlushAsync(capture));
        var actualException = ex is AggregateException agg ? agg.InnerException : ex;
        Assert.That(actualException, Is.InstanceOf<InvalidOperationException>());

        Assert.That(reportedErrors, Is.Not.Empty, "Genuine errors during parallel encoding must be reported to SaveErrors.");
        var reported = reportedErrors[0] is AggregateException aggReported ? aggReported.InnerException : reportedErrors[0];
        Assert.That(reported, Is.InstanceOf<InvalidOperationException>());
    }

    [TestCase(0)]
    [TestCase(-2)]
    public void Constructor_InvalidMaxParallelism_ThrowsArgumentOutOfRangeException(int invalidParallelism)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SessionRecoveryCoordinator(
            _storage,
            _codec,
            scheduler: _scheduler,
            maxParallelism: invalidParallelism));
    }

    [Test]
    public void Constructor_UnlimitedMaxParallelism_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new SessionRecoveryCoordinator(
            _storage,
            _codec,
            scheduler: _scheduler,
            maxParallelism: -1));
    }

    [Test]
    public void EncodePayloads_SinglePicture_ExecutesSequentialPath()
    {
        var dict = new Dictionary<string, Picture>
        {
            ["pic1"] = Picture.CreateEmpty(new PictureSize(8, 8))
        };

        var payloads = SessionRecoveryCoordinator.EncodePayloads(dict, _codec);
        Assert.That(payloads.Count, Is.EqualTo(1));
        Assert.That(payloads["pic1"], Is.Not.Empty);
    }

    [Test]
    public void EncodePayloads_AlreadyCanceledToken_WithEmptyPictures_ThrowsOperationCanceledException()
    {
        var dict = new Dictionary<string, Picture>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            SessionRecoveryCoordinator.EncodePayloads(dict, _codec, ct: cts.Token));
    }

    [TestCase(0)]
    [TestCase(-2)]
    public void EncodePayloads_InvalidConfiguredMaxParallelism_ThrowsArgumentOutOfRangeException(int invalidParallelism)
    {
        var dict = new Dictionary<string, Picture>
        {
            ["pic1"] = Picture.CreateEmpty(new PictureSize(8, 8))
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SessionRecoveryCoordinator.EncodePayloads(dict, _codec, configuredMaxParallelism: invalidParallelism));
    }

    [Test]
    public async Task FlushAsync_AlreadyCanceledToken_DoesNotCancelPriorRunningSave()
    {
        _storage.SimulatedDelay = TimeSpan.FromMilliseconds(150);

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            captureFactory: () => CreateCapture("doc-unused"),
            scheduler: _scheduler);

        var capture1 = CreateCapture("doc-valid");
        var task1 = coordinator.FlushAsync(capture1);

        await Task.Delay(20);

        using var canceledCts = new CancellationTokenSource();
        canceledCts.Cancel();

        var capture2 = CreateCapture("doc-canceled");
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await coordinator.FlushAsync(capture2, canceledCts.Token));

        await task1;

        Assert.That(_storage.SaveCount, Is.EqualTo(1));
        Assert.That(_storage.LatestSnapshot!.ActiveDocumentId, Is.EqualTo("doc-valid"));
    }

    [Test]
    public async Task Dispose_DuringActiveSave_CompletesGracefullyWithoutThrowingObjectDisposedException()
    {
        _storage.SimulatedDelay = TimeSpan.FromMilliseconds(200);

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            captureFactory: () => CreateCapture("doc-unused"),
            scheduler: _scheduler);

        var reportedErrors = new List<Exception>();
        using var sub = coordinator.SaveErrors.Subscribe(
            onNext: ex => reportedErrors.Add(ex),
            onError: ex => reportedErrors.Add(ex));

        var capture = CreateCapture("doc-disposing");
        var flushTask = coordinator.FlushAsync(capture);

        await Task.Delay(30);

        Assert.DoesNotThrow(() => coordinator.Dispose());

        try
        {
            await flushTask;
        }
        catch (Exception ex)
        {
            Assert.That(ex, Is.InstanceOf<OperationCanceledException>(), $"Expected OperationCanceledException, but got {ex.GetType().Name}: {ex.Message}");
        }

        // None of the reported errors should be ObjectDisposedException
        Assert.That(reportedErrors.OfType<ObjectDisposedException>(), Is.Empty);
    }

    private class MultiFailingPictureCodec : Eede.Application.Pictures.IPictureCodec
    {
        public Picture DecodeFromPng(byte[] bytes) => throw new NotImplementedException();

        public byte[] EncodeToPng(Picture picture)
        {
            throw new InvalidOperationException($"Codec failed for {picture.Width}x{picture.Height}");
        }
    }

    [Test]
    public void EncodePayloads_MultipleExceptions_ThrowsAggregateException()
    {
        var dict = new Dictionary<string, Picture>();
        for (int i = 0; i < 4; i++)
        {
            dict[$"doc_{i}"] = Picture.CreateEmpty(new PictureSize(16 + i, 16));
        }

        var multiFailingCodec = new MultiFailingPictureCodec();

        var ex = Assert.Catch<Exception>(() =>
            SessionRecoveryCoordinator.EncodePayloads(dict, multiFailingCodec, configuredMaxParallelism: 4));

        Assert.That(ex, Is.InstanceOf<AggregateException>());
        var agg = (AggregateException)ex!;
        Assert.That(agg.InnerExceptions.Count, Is.GreaterThan(1));
    }

    [Test]
    public async Task Concurrent_RapidFlushAsync_EnsuresLatestSnapshotWins()
    {
        _storage.SimulatedDelay = TimeSpan.FromMilliseconds(50);

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            scheduler: _scheduler);

        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            var capture = CreateCapture($"doc-{i}");
            tasks.Add(coordinator.FlushAsync(capture));
            await Task.Delay(10);
        }

        await Task.WhenAll(tasks);

        Assert.That(_storage.LatestSnapshot, Is.Not.Null);
        Assert.That(_storage.LatestSnapshot!.ActiveDocumentId, Is.EqualTo("doc-4"));
    }

    [Test]
    public async Task FlushAsync_WhenSuperseded_AwaitingFirstFlush_GuaranteesSessionIsSaved()
    {
        _storage.SimulatedDelay = TimeSpan.FromMilliseconds(100);

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            scheduler: _scheduler);

        var capture1 = CreateCapture("doc-1");
        var capture2 = CreateCapture("doc-2");

        var task1 = coordinator.FlushAsync(capture1);
        await Task.Delay(20);
        var task2 = coordinator.FlushAsync(capture2);

        // task1 を await する。task1 が正常完了したと主張するなら、ストレージへの保存が完了していなければならない！
        await task1;

        Assert.That(_storage.SaveCount, Is.GreaterThanOrEqualTo(1), "When FlushAsync completes without exception, storage save must have actually completed!");
        Assert.That(_storage.LatestSnapshot, Is.Not.Null);
        Assert.That(_storage.LatestSnapshot!.ActiveDocumentId, Is.EqualTo("doc-2"));
    }

    [Test]
    public async Task FlushAsync_MultipleSupersededChain_AllPriorTasksCompleteSuccessfullyAndSaveLatest()
    {
        _storage.SimulatedDelay = TimeSpan.FromMilliseconds(80);

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            scheduler: _scheduler);

        var task1 = coordinator.FlushAsync(CreateCapture("chain-1"));
        await Task.Delay(15);
        var task2 = coordinator.FlushAsync(CreateCapture("chain-2"));
        await Task.Delay(15);
        var task3 = coordinator.FlushAsync(CreateCapture("chain-3"));

        await Task.WhenAll(task1, task2, task3);

        Assert.That(_storage.LatestSnapshot, Is.Not.Null);
        Assert.That(_storage.LatestSnapshot!.ActiveDocumentId, Is.EqualTo("chain-3"));
    }

    [Test]
    public async Task FlushAsync_WhenSupersedingTaskFails_PriorTasksPropagateException()
    {
        _storage.SimulatedDelay = TimeSpan.FromMilliseconds(50);

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            scheduler: _scheduler);

        var task1 = coordinator.FlushAsync(CreateCapture("doc-ok"));
        await Task.Delay(15);

        // 後続タスクの直前にストレージを失敗するように設定
        _storage.SimulatedException = new InvalidOperationException("Disk write failure");
        var task2 = coordinator.FlushAsync(CreateCapture("doc-failing"));

        // task1 も後続タスクの失敗により例外を受け取らなければならない
        var ex1 = Assert.CatchAsync<Exception>(async () => await task1);
        var ex2 = Assert.CatchAsync<Exception>(async () => await task2);

        var inner1 = ex1 is AggregateException agg1 ? agg1.InnerException : ex1;
        var inner2 = ex2 is AggregateException agg2 ? agg2.InnerException : ex2;

        Assert.That(inner1, Is.InstanceOf<InvalidOperationException>());
        Assert.That(inner2, Is.InstanceOf<InvalidOperationException>());
        Assert.That(inner1!.Message, Is.EqualTo("Disk write failure"));
    }

    [Test]
    public async Task FlushAsync_ExternalCancellationDuringSupersededWait_ThrowsImmediatelyWithoutWaiting()
    {
        _storage.SimulatedDelay = TimeSpan.FromMilliseconds(300);

        var coordinator = new SessionRecoveryCoordinator(
            _storage,
            _codec,
            scheduler: _scheduler);

        using var cts1 = new CancellationTokenSource();
        var task1 = coordinator.FlushAsync(CreateCapture("doc-1"), cts1.Token);
        await Task.Delay(20);

        // task2 を開始して task1 を Superseded にする
        var task2 = coordinator.FlushAsync(CreateCapture("doc-2"));
        await Task.Delay(20);

        // task1 の外部トークンをキャンセル
        cts1.Cancel();

        // task1 は task2 (300ms) の完了を待たずに直ちにキャンセル例外を投げること
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var ex = Assert.CatchAsync<OperationCanceledException>(async () => await task1);
        sw.Stop();

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex, Is.InstanceOf<OperationCanceledException>());
        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(200), "Task1 should have aborted immediately upon external cancellation rather than waiting for task2.");

        // task2 自体は正常に完了すること
        await task2;
        Assert.That(_storage.LatestSnapshot!.ActiveDocumentId, Is.EqualTo("doc-2"));
    }
}

