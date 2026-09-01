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
}
