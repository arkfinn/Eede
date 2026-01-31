using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Application.Infrastructure;
using Eede.Presentation.Common.Services;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.IO;
using Eede.Presentation.Files;

namespace Eede.Presentation.ViewModels.Animations;

public class AnimationViewModel : ViewModelBase, IAddFrameProvider
{
    private readonly IAnimationService _animationService;
    private readonly IFileSystem _fileSystem;
    private readonly IImageTransfer _imageTransfer = new DirectImageTransfer();

    [Reactive] public AnimationPattern? SelectedPattern { get; set; }
    public ObservableCollection<AnimationPattern> Patterns { get; } = new();

    [Reactive] public bool IsPlaying { get; set; }
    [Reactive] public int CurrentFrameIndex { get; set; }
    private readonly ObservableAsPropertyHelper<AnimationFrame?>? _currentFrame;
    public AnimationFrame? CurrentFrame => _currentFrame?.Value;

    [Reactive] public bool IsAnimationMode { get; set; }
    [Reactive] public int GridWidth { get; set; }
    [Reactive] public int GridHeight { get; set; }
    public ObservableCollection<int> GridSizeList { get; } = new([8, 16, 24, 32, 48, 64]);

    [Reactive] public int WaitTime { get; set; }

    [Reactive] public Magnification Magnification { get; set; }
    [Reactive] public Picture? ActivePicture { get; set; }
    [Reactive] public Bitmap? PreviewBitmap { get; set; }

    public ReactiveCommand<string, Unit> CreatePatternCommand { get; }
    public ReactiveCommand<Unit, Unit> RemovePatternCommand { get; }
    public ReactiveCommand<int, Unit> AddFrameCommand { get; }
    public ReactiveCommand<AnimationFrame, Unit> RemoveFrameAtCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearSequenceCommand { get; }
    public ReactiveCommand<Unit, Unit> TogglePlayCommand { get; }
    public ReactiveCommand<IFileStorage, Unit> ExportCommand { get; }
    public ReactiveCommand<IFileStorage, Unit> ImportCommand { get; }

    public void AddFrame(int cellIndex)
    {
        AddFrameCommand.Execute(cellIndex).Subscribe();
    }

    public AnimationViewModel(IAnimationService animationService, IFileSystem fileSystem)
    {
        _animationService = animationService;
        _fileSystem = fileSystem;

        _currentFrame = this.WhenAnyValue(x => x.SelectedPattern, x => x.CurrentFrameIndex)
            .Select(x => (x.Item1 != null && x.Item2 >= 0 && x.Item2 < x.Item1.Frames.Count)
                ? x.Item1.Frames[x.Item2]
                : null)
            .ToProperty(this, x => x.CurrentFrame, scheduler: RxApp.MainThreadScheduler);

        GridWidth = 32;
        GridHeight = 32;
        WaitTime = 100;
        Magnification = new Magnification(4);

        foreach (var pattern in _animationService.Patterns)
        {
            Patterns.Add(pattern);
        }

        if (Patterns.Count == 0)
        {
            var testPattern = new AnimationPattern("Test Run", new List<AnimationFrame>
            {
                new AnimationFrame(0, 100),
                new AnimationFrame(1, 100),
                new AnimationFrame(2, 100),
                new AnimationFrame(1, 100)
            }, new GridSettings(new PictureSize(GridWidth, GridHeight), new Position(0, 0), 0));
            _animationService.Add(testPattern);
            Patterns.Add(testPattern);
            SelectedPattern = testPattern;
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
                    var newFrames = SelectedPattern.Frames.Select(f => new AnimationFrame(f.CellIndex, waitTime)).ToList();
                    var newPattern = new AnimationPattern(SelectedPattern.Name, newFrames, SelectedPattern.Grid);
                    UpdatePattern(newPattern);
                }
            });

        CreatePatternCommand = ReactiveCommand.Create<string>(name =>
        {
            var newPattern = new AnimationPattern(name, new List<AnimationFrame>(), new GridSettings(new PictureSize(GridWidth, GridHeight), new Position(0, 0), 0));
            _animationService.Add(newPattern);
            Patterns.Add(newPattern);
            SelectedPattern = newPattern;
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
                    _animationService.Remove(index);
                    Patterns.RemoveAt(index);
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
                var frames = SelectedPattern.Frames.ToList();
                int index = frames.IndexOf(frame);
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
                if (picture != null && frame != null && SelectedPattern != null && mag != null)
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
                        PreviewBitmap = PictureBitmapAdapter.ConvertToPremultipliedBitmap(magnified);
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

        ExportCommand = ReactiveCommand.CreateFromTask<IFileStorage>(async storage =>
        {
            if (SelectedPattern == null) return;
            var uri = await storage.SaveAnimationFilePickerAsync();
            if (uri == null) return;

            var json = JsonSerializer.Serialize(SelectedPattern);
            await _fileSystem.WriteAllTextAsync(uri.LocalPath, json);
        });

        ImportCommand = ReactiveCommand.CreateFromTask<IFileStorage>(async storage =>
        {
            var uri = await storage.OpenFilePickerAsync();
            if (uri == null) return;

            var json = await _fileSystem.ReadAllTextAsync(uri.LocalPath);
            var pattern = JsonSerializer.Deserialize<AnimationPattern>(json);
            if (pattern != null)
            {
                _animationService.Add(pattern);
                Patterns.Add(pattern);
                SelectedPattern = pattern;
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
                    },
                    RxApp.MainThreadScheduler
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

    private void UpdatePattern(AnimationPattern newPattern)
    {
        if (SelectedPattern == null) return;
        int index = Patterns.IndexOf(SelectedPattern);
        if (index >= 0)
        {
            _animationService.Replace(index, newPattern);
            Patterns[index] = newPattern;
            SelectedPattern = newPattern;
        }
    }
}
