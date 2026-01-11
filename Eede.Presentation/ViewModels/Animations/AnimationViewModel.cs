using Eede.Application.Animations;
using Eede.Domain.Animations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace Eede.Presentation.ViewModels.Animations;

public class AnimationViewModel : ViewModelBase
{
    private readonly IAnimationService _animationService;

    [Reactive] public AnimationPattern? SelectedPattern { get; set; }
    public ObservableCollection<AnimationPattern> Patterns { get; } = new();

    [Reactive] public bool IsPlaying { get; set; }
    [Reactive] public int CurrentFrameIndex { get; set; }
    private readonly ObservableAsPropertyHelper<AnimationFrame?> _currentFrame;
    public AnimationFrame? CurrentFrame => _currentFrame.Value;

    public ReactiveCommand<string, Unit> CreatePatternCommand { get; }
    public ReactiveCommand<Unit, Unit> RemovePatternCommand { get; }
    public ReactiveCommand<int, Unit> AddFrameCommand { get; }
    public ReactiveCommand<Unit, Unit> TogglePlayCommand { get; }

    public AnimationViewModel(IAnimationService animationService)
    {
        _animationService = animationService;

        foreach (var pattern in _animationService.Patterns)
        {
            Patterns.Add(pattern);
        }

        CreatePatternCommand = ReactiveCommand.Create<string>(name =>
        {
            var newPattern = new AnimationPattern(name, new System.Collections.Generic.List<AnimationFrame>(), new GridSettings(new(32, 32), new(0, 0), 0));
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
                var newPattern = SelectedPattern.AddFrame(new AnimationFrame(cellIndex, 100));
                UpdatePattern(newPattern);
            }
        }, canExecute);

        TogglePlayCommand = ReactiveCommand.Create(() =>
        {
            IsPlaying = !IsPlaying;
        }, canExecute);

        this.WhenAnyValue(x => x.SelectedPattern)
            .Subscribe(_ => CurrentFrameIndex = 0);

        _currentFrame = this.WhenAnyValue(x => x.SelectedPattern, x => x.CurrentFrameIndex)
            .Select(x => (x.Item1 != null && x.Item2 >= 0 && x.Item2 < x.Item1.Frames.Count)
                ? x.Item1.Frames[x.Item2]
                : null)
            .ToProperty(this, x => x.CurrentFrame);

        this.WhenAnyValue(x => x.IsPlaying, x => x.SelectedPattern)
            .Select(x => x.Item1 && x.Item2 != null && x.Item2.Frames.Count > 0)
            .Select(playing => playing 
                ? Observable.Defer(() =>
                {
                    var frame = SelectedPattern!.Frames[CurrentFrameIndex % SelectedPattern.Frames.Count];
                    return Observable.Timer(TimeSpan.FromMilliseconds(frame.Duration), RxApp.MainThreadScheduler);
                }).Repeat()
                : Observable.Empty<long>())
            .Switch()
            .ObserveOn(RxApp.MainThreadScheduler)
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
