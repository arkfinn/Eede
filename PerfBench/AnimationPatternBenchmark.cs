using BenchmarkDotNet.Attributes;
using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using System.Collections.Generic;

namespace PerfBench;

[MemoryDiagnoser]
public class AnimationPatternBenchmark
{
    [Params(4, 16, 64)]
    public int FrameCount { get; set; }

    private AnimationPattern _pattern = default!;
    private AnimationFrame _newFrame = default!;

    [GlobalSetup]
    public void Setup()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var frames = new List<AnimationFrame>();
        for (int i = 0; i < FrameCount; i++)
        {
            frames.Add(new AnimationFrame(i, 100));
        }
        _pattern = new AnimationPattern("Test", frames, grid);
        _newFrame = new AnimationFrame(999, 100);
    }

    [Benchmark]
    public int ReadFramesByIndex()
    {
        int totalDuration = 0;
        var frames = _pattern.Frames;
        for (int i = 0; i < frames.Count; i++)
        {
            totalDuration += frames[i].Duration;
        }
        return totalDuration;
    }

    [Benchmark]
    public AnimationPattern AddFrame()
    {
        return _pattern.AddFrame(_newFrame);
    }

    [Benchmark]
    public AnimationPattern RemoveFrame()
    {
        return _pattern.RemoveFrame(FrameCount / 2);
    }

    [Benchmark]
    public AnimationPattern UpdateFrame()
    {
        return _pattern.UpdateFrame(FrameCount / 2, _newFrame);
    }

    [Benchmark]
    public AnimationPattern MoveFrame()
    {
        return _pattern.MoveFrame(0, FrameCount - 1);
    }
}
