#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Recovery;

namespace Eede.Application.Recovery;

public sealed class SessionRecoveryCoordinator : IDisposable
{
    private readonly ISessionStorage _storage;
    private readonly IPictureCodec _codec;
    private Func<SessionCapture?>? _captureFactory;
    private readonly TimeSpan _debounceDuration;
    private readonly IScheduler _scheduler;
    private readonly int? _configuredMaxParallelism;

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
        IScheduler? scheduler = null,
        int? maxParallelism = null)
    {
        if (maxParallelism is <= 0 and not -1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxParallelism), "maxParallelism must be greater than 0, or -1 for unlimited.");
        }

        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _codec = codec ?? throw new ArgumentNullException(nameof(codec));
        _captureFactory = captureFactory;
        _debounceDuration = debounceDuration ?? TimeSpan.FromSeconds(1.5);
        _scheduler = scheduler ?? TaskPoolScheduler.Default;
        _configuredMaxParallelism = maxParallelism;

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

    /// <summary>
    /// Encodes a dictionary of pictures to PNG format, utilizing parallel execution when multiple pictures are present.
    /// </summary>
    /// <param name="pictures">The pictures to encode.</param>
    /// <param name="codec">The picture codec used for PNG encoding.</param>
    /// <param name="configuredMaxParallelism">Configured max degree of parallelism (-1 for unlimited, null for CPU/2 clamped to 1..4).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A dictionary mapping payload keys to encoded PNG byte arrays.</returns>
    public static IReadOnlyDictionary<string, byte[]> EncodePayloads(
        IReadOnlyDictionary<string, Picture> pictures,
        IPictureCodec codec,
        int? configuredMaxParallelism = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pictures);
        ArgumentNullException.ThrowIfNull(codec);

        var effectiveParallelism = configuredMaxParallelism switch
        {
            -1 => Environment.ProcessorCount,
            int p => p,
            null => Math.Clamp(Environment.ProcessorCount / 2, 1, 4)
        };

        if (pictures.Count <= 1 || effectiveParallelism <= 1)
        {
            var dict = new Dictionary<string, byte[]>(pictures.Count);
            foreach (var (key, picture) in pictures)
            {
                ct.ThrowIfCancellationRequested();
                dict[key] = codec.EncodeToPng(picture);
            }
            return dict;
        }

        var maxParallelism = Math.Min(pictures.Count, effectiveParallelism);
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = ct,
            MaxDegreeOfParallelism = maxParallelism
        };

        var concurrentDict = new ConcurrentDictionary<string, byte[]>(maxParallelism, pictures.Count);
        try
        {
            Parallel.ForEach(pictures, parallelOptions, kvp =>
            {
                parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                var encoded = codec.EncodeToPng(kvp.Value);
                concurrentDict[kvp.Key] = encoded;
            });
        }
        catch (AggregateException ex)
        {
            var nonCancelExceptions = ex.Flatten().InnerExceptions
                .Where(e => e is not OperationCanceledException)
                .ToList();

            if (nonCancelExceptions.Count == 1)
            {
                ExceptionDispatchInfo.Capture(nonCancelExceptions[0]).Throw();
            }
            else if (nonCancelExceptions.Count > 1)
            {
                throw new AggregateException(nonCancelExceptions);
            }

            ct.ThrowIfCancellationRequested();
            throw new OperationCanceledException("Parallel encoding canceled.", ex, ct);
        }

        return concurrentDict;
    }

    private async Task ExecuteSaveAsync(SessionCapture capture, CancellationToken externalCt, bool throwOnError)
    {
        CancellationTokenSource linkedCts;
        lock (_ctsLock)
        {
            if (_isDisposed) return;

            // 先行タスクをキャンセル
            if (_activeSaveCts is not null)
            {
                try
                {
                    _activeSaveCts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }
            }

            _activeSaveCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            linkedCts = _activeSaveCts;
        }

        var ct = linkedCts.Token;

        bool IsSuperseded()
        {
            lock (_ctsLock)
            {
                return !ReferenceEquals(_activeSaveCts, linkedCts);
            }
        }

        try
        {
            try
            {
                await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // セマフォ待機中にキャンセルされた場合:
                // 後続タスクによって置き換えられた（Superseded）なら静かに退場。
                // 外部トークン要求や自発的キャンセルかつ throwOnError の場合は例外を再送。
                if (throwOnError && !IsSuperseded())
                {
                    throw;
                }
                return;
            }

            try
            {
                ct.ThrowIfCancellationRequested();

                // Phase 2: CPUバウンドな画像エンコードをスレッドプールでオフロード
                var encodedPayloads = await Task.Run(
                    () => EncodePayloads(capture.Pictures, _codec, _configuredMaxParallelism, ct),
                    ct).ConfigureAwait(false);

                // Phase 3: I/Oバウンドなストレージ保存
                ct.ThrowIfCancellationRequested();
                await _storage.SaveSnapshotAsync(capture.Snapshot, encodedPayloads, ct).ConfigureAwait(false);

                _snapshotSavedSubject.OnNext(capture.Snapshot);
            }
            catch (OperationCanceledException)
            {
                // 実行中にキャンセルされた場合:
                // 後続タスクによって置き換えられた（Superseded）なら静かに退場。
                // それ以外のキャンセルで throwOnError なら再スロー。
                if (throwOnError && !IsSuperseded())
                {
                    throw;
                }
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
                try
                {
                    _semaphore.Release();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }
        finally
        {
            lock (_ctsLock)
            {
                if (ReferenceEquals(_activeSaveCts, linkedCts))
                {
                    _activeSaveCts = null;
                }
            }
            linkedCts.Dispose();
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
            if (_activeSaveCts is not null)
            {
                try
                {
                    _activeSaveCts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }
                _activeSaveCts = null;
            }
        }

        _disposables.Dispose();
        _semaphore.Dispose();
    }
}
