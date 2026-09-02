#nullable enable
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing.Recovery;

namespace Eede.Application.Recovery;

public sealed class SessionRecoveryCoordinator : IDisposable
{
    private readonly ISessionStorage _storage;
    private readonly IPictureCodec _codec;
    private Func<SessionCapture?>? _captureFactory;
    private readonly TimeSpan _debounceDuration;
    private readonly IScheduler _scheduler;

    private readonly Subject<Unit> _dirtySubject = new();
    private readonly Subject<SessionSnapshot> _snapshotSavedSubject = new();
    private readonly Subject<Exception> _errorSubject = new();
    private readonly CompositeDisposable _disposables = new();

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly object _ctsLock = new();
    private CancellationTokenSource? _activeSaveCts;
    private Task? _lastSaveTask;
    private bool _isDisposed;

    public IObservable<SessionSnapshot> SnapshotSaved => _snapshotSavedSubject.AsObservable();
    public IObservable<Exception> SaveErrors => _errorSubject.AsObservable();
    public Task? LastSaveTask => _lastSaveTask;

    public SessionRecoveryCoordinator(
        ISessionStorage storage,
        IPictureCodec codec,
        Func<SessionCapture?>? captureFactory = null,
        IObservable<object>? dirtyStream = null,
        TimeSpan? debounceDuration = null,
        IScheduler? scheduler = null)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _codec = codec ?? throw new ArgumentNullException(nameof(codec));
        _captureFactory = captureFactory;
        _debounceDuration = debounceDuration ?? TimeSpan.FromSeconds(1.5);
        _scheduler = scheduler ?? TaskPoolScheduler.Default;

        var mergedDirty = _dirtySubject.AsObservable();
        if (dirtyStream is not null)
        {
            mergedDirty = mergedDirty.Merge(dirtyStream.Select(_ => Unit.Default));
        }

        var debouncedSubscription = mergedDirty
            .Throttle(_debounceDuration, _scheduler)
            .Subscribe(_ =>
            {
                if (_isDisposed) return;

                // Phase 1: スナップショット抽出 (UIスレッド/呼び出し側コンテキスト同期)
                SessionCapture? capture;
                try
                {
                    capture = _captureFactory?.Invoke();
                }
                catch (Exception ex)
                {
                    _errorSubject.OnNext(ex);
                    return;
                }

                if (capture is null) return;

                // Phase 2: オフロード & 直列先行キャンセル保存
                _lastSaveTask = ExecuteSaveAsync(capture, CancellationToken.None, throwOnError: false);
            });

        _disposables.Add(debouncedSubscription);
        _disposables.Add(_dirtySubject);
        _disposables.Add(_snapshotSavedSubject);
        _disposables.Add(_errorSubject);
    }

    public void SetCaptureFactory(Func<SessionCapture?> captureFactory)
    {
        ThrowIfDisposed();
        _captureFactory = captureFactory ?? throw new ArgumentNullException(nameof(captureFactory));
    }

    public void NotifyDirty()
    {
        ThrowIfDisposed();
        _dirtySubject.OnNext(Unit.Default);
    }

    public async Task FlushAsync(SessionCapture? directCapture = null, CancellationToken ct = default)
    {
        ThrowIfDisposed();

        // Phase 1: スナップショット抽出
        var capture = directCapture ?? _captureFactory?.Invoke();
        if (capture is null) return;

        // Phase 2: 直ちに実行・待機
        var task = ExecuteSaveAsync(capture, ct, throwOnError: true);
        _lastSaveTask = task;
        await task.ConfigureAwait(false);
    }

    private async Task ExecuteSaveAsync(SessionCapture capture, CancellationToken externalCt, bool throwOnError)
    {
        CancellationTokenSource linkedCts;
        lock (_ctsLock)
        {
            if (_isDisposed) return;

            // 先行タスクをキャンセル
            _activeSaveCts?.Cancel();
            _activeSaveCts?.Dispose();

            _activeSaveCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            linkedCts = _activeSaveCts;
        }

        var ct = linkedCts.Token;

        try
        {
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // セマフォ待機中に最新のリクエストにより先行キャンセルされた
            return;
        }

        try
        {
            ct.ThrowIfCancellationRequested();

            // Phase 2: Taskpool 非同期オフロード
            await Task.Run(async () =>
            {
                var encodedPayloads = new Dictionary<string, byte[]>();
                foreach (var (key, picture) in capture.Pictures)
                {
                    ct.ThrowIfCancellationRequested();
                    var encoded = _codec.EncodeToPng(picture);
                    encodedPayloads[key] = encoded;
                }

                ct.ThrowIfCancellationRequested();
                await _storage.SaveSnapshotAsync(capture.Snapshot, encodedPayloads, ct).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);

            _snapshotSavedSubject.OnNext(capture.Snapshot);
        }
        catch (OperationCanceledException)
        {
            // キャンセルされた場合は正常な中断として扱う
        }
        catch (Exception ex)
        {
            _errorSubject.OnNext(ex);
            if (throwOnError)
            {
                throw;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        lock (_ctsLock)
        {
            _activeSaveCts?.Cancel();
            _activeSaveCts?.Dispose();
            _activeSaveCts = null;
        }

        _disposables.Dispose();
        _semaphore.Dispose();
    }
}
