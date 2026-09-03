using Eede.Application.Animations;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Animations;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Files;
using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using RxVoid = ReactiveUI.Primitives.RxVoid;
using System.Reactive.Linq;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using ReactiveUI.SourceGenerators;

namespace Eede.Presentation.ViewModels.Animations;

#nullable enable

public partial class AnimationViewModel : ViewModelBase, IAddFrameProvider
{
    private readonly IAnimationPatternsProvider _patternsProvider;
    private readonly IAnimationPatternEditor _patternEditor;
    private readonly IFileSystem _fileSystem;
    private readonly IBitmapAdapter<Bitmap> _bitmapAdapter;
    private readonly IImageTransfer _imageTransfer = new DirectImageTransfer();

    [Reactive] public partial AnimationPattern? SelectedPattern { get; set; }
    public ObservableCollection<AnimationPattern> Patterns { get; } = new();

    [Reactive] public partial bool IsPlaying { get; set; }
    [Reactive] public partial int CurrentFrameIndex { get; set; }
    [ObservableAsProperty] private AnimationFrame? _currentFrame;

    [Reactive] public partial bool IsAnimationMode { get; set; }
    [Reactive] public partial int GridWidth { get; set; }
    [Reactive] public partial int GridHeight { get; set; }
    public ObservableCollection<int> GridSizeList { get; } = new([8, 16, 24, 32, 48, 64]);

    [Reactive] public partial int WaitTime { get; set; }

    public bool IsBrowserPlatform => OperatingSystem.IsBrowser();
    public bool IsDesktopPlatform => !OperatingSystem.IsBrowser();

    [Reactive] public partial Magnification Magnification { get; set; }
    [Reactive] public partial Picture? ActivePicture { get; set; }
    private Bitmap? _previewBitmap;
    public Bitmap? PreviewBitmap
    {
        get => _previewBitmap;
        set
        {
            if (_previewBitmap != value)
            {
                _previewBitmap?.Dispose();
            }
            _ = this.RaiseAndSetIfChanged(ref _previewBitmap, value);
        }
    }

    public ReactiveCommand<string, RxVoid> CreatePatternCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> RemovePatternCommand { get; }
    public ReactiveCommand<int, RxVoid> AddFrameCommand { get; }
    public ReactiveCommand<AnimationFrame, RxVoid> RemoveFrameAtCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> TogglePlayCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> NextFrameCommand { get; }
    public ReactiveCommand<RxVoid, RxVoid> PreviousFrameCommand { get; }
    public ReactiveCommand<IFileStorage, RxVoid> ExportCommand { get; }
    public ReactiveCommand<IFileStorage, RxVoid> ImportCommand { get; }

    public void AddFrame(int cellIndex)
    {
        // Safe execution when patterns might be empty
        AddFrameCommand.Execute(cellIndex).Subscribe();
    }

    public AnimationViewModel() : this(new AnimationPatternsProvider())
    {
    }

    private AnimationViewModel(IAnimationPatternsProvider provider) : this(
        provider,
        new AnimationPatternEditor(
            new AddAnimationPatternUseCase(provider),
            new ReplaceAnimationPatternUseCase(provider),
            new RemoveAnimationPatternUseCase(provider)),
        new AvaloniaFileSystem(),
        new AvaloniaBitmapAdapter())
    {
    }

    public AnimationViewModel(
        IAnimationPatternsProvider patternsProvider,
        IAnimationPatternEditor patternEditor,
        IFileSystem fileSystem,
        IBitmapAdapter<Bitmap> bitmapAdapter)
    {
        _patternsProvider = patternsProvider;
        _patternEditor = patternEditor;
        _fileSystem = fileSystem;
        _bitmapAdapter = bitmapAdapter;

        _currentFrameHelper = null!;

        this.WhenAnyValue(x => x.SelectedPattern, x => x.CurrentFrameIndex)
            .Select(x => (x.Item1 != null && x.Item2 >= 0 && x.Item2 < x.Item1.Frames.Count)
                ? x.Item1.Frames[x.Item2]
                : null)
            .ToProperty(this, nameof(CurrentFrame), out _currentFrameHelper);

        Magnification = new Magnification(1);
        GridWidth = 32;
        GridHeight = 32;
        WaitTime = 100;
        Magnification = new Magnification(4);

        SyncPatterns(_patternsProvider.Current);
        _patternsProvider.Changed += SyncPatterns;

        if (Patterns.Count == 0)
        {
            var testPattern = new AnimationPattern("Test Run", new List<AnimationFrame>
            {
                new AnimationFrame(0, 100),
                new AnimationFrame(1, 100),
                new AnimationFrame(2, 100),
                new AnimationFrame(1, 100)
            }, new GridSettings(new PictureSize(GridWidth, GridHeight), new Position(0, 0), 0));
            _patternEditor.Add(testPattern);
            SelectedPattern = Patterns.FirstOrDefault();
        }

        this.WhenAnyValue(x => x.SelectedPattern)
            .Where(x => x != null)
            .Subscribe(x =>
            {
                GridWidth = x!.Grid.CellSize.Width;
                GridHeight = x!.Grid.CellSize.Height;
                if (x.Frames.Count > 0)
                {
                    WaitTime = x.Frames[0].Duration;
                }
                CurrentFrameIndex = 0;
            });

        this.WhenAnyValue(x => x.GridWidth, x => x.GridHeight)
            .Subscribe(x =>
            {
                if (SelectedPattern != null && (SelectedPattern.Grid.CellSize.Width != x.Item1 || SelectedPattern.Grid.CellSize.Height != x.Item2))
                {
                    var newPattern = new AnimationPattern(
                        SelectedPattern.Name,
                        SelectedPattern.Frames,
                        new GridSettings(new PictureSize(x.Item1, x.Item2), SelectedPattern.Grid.Offset, SelectedPattern.Grid.Padding));
                    UpdatePattern(newPattern);
                }
            });

        this.WhenAnyValue(x => x.WaitTime)
            .Subscribe(waitTime =>
            {
                if (SelectedPattern != null)
                {
                    var count = SelectedPattern.Frames.Count;
                    var newFrames = new AnimationFrame[count];
                    for (int i = 0; i < count; i++)
                    {
                        newFrames[i] = new AnimationFrame(SelectedPattern.Frames[i].CellIndex, waitTime);
                    }
                    var newPattern = new AnimationPattern(SelectedPattern.Name, newFrames, SelectedPattern.Grid);
                    UpdatePattern(newPattern);
                }
            });

        CreatePatternCommand = ReactiveCommand.Create<string>(name =>
        {
            var newPattern = new AnimationPattern(name, new List<AnimationFrame>(), new GridSettings(new PictureSize(GridWidth, GridHeight), new Position(0, 0), 0));
            _patternEditor.Add(newPattern);
            SelectedPattern = Patterns.LastOrDefault();
        });

        var canExecute = this.WhenAnyValue(x => x.SelectedPattern)
            .Select(x => x != null);

        RemovePatternCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedPattern != null)
            {
                int index = Patterns.IndexOf(SelectedPattern);
                if (index >= 0)
                {
                    _patternEditor.Remove(index);
                    SelectedPattern = Patterns.Count > 0 ? Patterns[0] : null;
                }
            }
        }, canExecute);

        AddFrameCommand = ReactiveCommand.Create<int>(cellIndex =>
        {
            if (SelectedPattern != null)
            {
                var newPattern = SelectedPattern.AddFrame(new AnimationFrame(cellIndex, WaitTime));
                UpdatePattern(newPattern);
            }
        }, canExecute);

        RemoveFrameAtCommand = ReactiveCommand.Create<AnimationFrame>(frame =>
        {
            if (SelectedPattern != null && frame != null)
            {
                var frames = SelectedPattern.Frames;
                int index = -1;
                for (int i = 0; i < frames.Count; i++)
                {
                    if (frames[i] == frame)
                    {
                        index = i;
                        break;
                    }
                }
                if (index >= 0)
                {
                    var newPattern = SelectedPattern.RemoveFrame(index);
                    UpdatePattern(newPattern);
                }
            }
        }, canExecute);

        this.WhenAnyValue(x => x.ActivePicture, x => x.CurrentFrame, x => x.Magnification)
            .Subscribe(x =>
            {
                var picture = x.Item1;
                var frame = x.Item2;
                var mag = x.Item3;
                if (picture != null && frame != null && SelectedPattern != null)
                {
                    var cellSize = SelectedPattern.Grid.CellSize;
                    var offset = SelectedPattern.Grid.Offset;
                    var padding = SelectedPattern.Grid.Padding;
                    int columns = Math.Max(1, (picture.Size.Width - offset.X + padding) / (cellSize.Width + padding));

                    int col = frame.CellIndex % columns;
                    int row = frame.CellIndex / columns;

                    var rect = new PictureArea(
                        new Position(offset.X + col * (cellSize.Width + padding), offset.Y + row * (cellSize.Height + padding)),
                        cellSize);

                    if (picture.Contains(rect.Position) && rect.X + rect.Width <= picture.Width && rect.Y + rect.Height <= picture.Height)
                    {
                        var framePixels = picture.CutOut(rect);
                        var magnified = _imageTransfer.Transfer(framePixels, mag);
                        PreviewBitmap = _bitmapAdapter.ConvertToPremultipliedBitmap(magnified);
                    }
                    else
                    {
                        PreviewBitmap = null;
                    }
                }
                else
                {
                    PreviewBitmap = null;
                }
            });

        TogglePlayCommand = ReactiveCommand.Create(() => { IsPlaying = !IsPlaying; });
        NextFrameCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedPattern != null && SelectedPattern.Frames.Count > 0)
            {
                CurrentFrameIndex = (CurrentFrameIndex + 1) % SelectedPattern.Frames.Count;
            }
        });
        PreviousFrameCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedPattern != null && SelectedPattern.Frames.Count > 0)
            {
                CurrentFrameIndex = (CurrentFrameIndex - 1 + SelectedPattern.Frames.Count) % SelectedPattern.Frames.Count;
            }
        });

        ExportCommand = ReactiveCommand.CreateFromTask<IFileStorage>(async storage =>
        {
            if (SelectedPattern == null) return;
            var uri = await storage.SaveAnimationFilePickerAsync();
            if (uri == null) return;

            try
            {
                var json = JsonSerializer.Serialize(SelectedPattern);
                if (!IsBrowserPlatform && uri.IsAbsoluteUri && uri.IsFile)
                {
                    await _fileSystem.WriteAllTextAsync(uri.LocalPath, json);
                    return;
                }

                await using var stream = await storage.OpenWriteStreamAsync(uri);
                using var writer = new StreamWriter(stream);
                await writer.WriteAsync(json);
                await writer.FlushAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to export animation file: {ex.Message}");
            }
        });

        ImportCommand = ReactiveCommand.CreateFromTask<IFileStorage>(async storage =>
        {
            var uri = await storage.OpenAnimationFilePickerAsync();
            if (uri == null) return;

            try
            {
                string json;
                if (!IsBrowserPlatform && uri.IsAbsoluteUri && uri.IsFile)
                {
                    json = await _fileSystem.ReadAllTextAsync(uri.LocalPath);
                }
                else
                {
                    await using var stream = await storage.OpenReadStreamAsync(uri);
                    using var reader = new StreamReader(stream);
                    json = await reader.ReadToEndAsync();
                }

                var pattern = JsonSerializer.Deserialize<AnimationPattern>(json);
                if (pattern != null && pattern.Validate())
                {
                    _patternEditor.Add(pattern);
                    SelectedPattern = Patterns.LastOrDefault();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to import animation file: {ex.Message}");
            }
        });

        this.WhenAnyValue(x => x.IsPlaying, x => x.SelectedPattern)
            .Select(x =>
            {
                bool playing = x.Item1;
                var pattern = x.Item2;
                if (!playing || pattern == null || pattern.Frames.Count == 0)
                {
                    return Observable.Empty<int>();
                }

                // 各フレームのDurationを考慮したタイマーを生成
                return Observable.Generate(
                    0, // 初期状態（ダミー）
                    _ => true,
                    _ => 0, // インクリメントはSubscribe内で行うためダミー
                    _ => 0, // 同上
                    _ =>
                    {
                        if (SelectedPattern == null || CurrentFrameIndex < 0 || CurrentFrameIndex >= SelectedPattern.Frames.Count)
                        {
                            return TimeSpan.FromMilliseconds(100);
                        }
                        return TimeSpan.FromMilliseconds(SelectedPattern.Frames[CurrentFrameIndex].Duration);
                    }
                );
            })
            .Switch()
            .Subscribe(_ =>
            {
                if (SelectedPattern != null && SelectedPattern.Frames.Count > 0)
                {
                    CurrentFrameIndex = (CurrentFrameIndex + 1) % SelectedPattern.Frames.Count;
                }
            });
    }

    private void SyncPatterns(AnimationPatterns patterns)
    {
        // シンプルな同期（必要に応じて最適化）
        Patterns.Clear();
        foreach (var p in patterns.Items)
        {
            Patterns.Add(p);
        }
    }

    private void UpdatePattern(AnimationPattern newPattern)
    {
        if (SelectedPattern == null) return;
        int index = Patterns.IndexOf(SelectedPattern);
        if (index >= 0)
        {
            _patternEditor.Replace(index, newPattern);
            SelectedPattern = Patterns[index];
        }
    }
}

