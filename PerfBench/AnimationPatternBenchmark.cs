using BenchmarkDotNet.Attributes;
using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using System.Collections.Generic;

namespace PerfBench;

[MemoryDiagnoser]
public class AnimationPatternBenchmark
{
    private AnimationPattern _pattern = default!;
    private AnimationFrame _newFrame = default!;

    [GlobalSetup]
    public void Setup()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var frames = new List<AnimationFrame>();
        for (int i = 0; i < 100; i++)
        {
            frames.Add(new AnimationFrame(i, 100));
        }
        _pattern = new AnimationPattern("Test", frames, grid);
        _newFrame = new AnimationFrame(100, 100);
    }

    [Benchmark]
    public AnimationPattern AddFrame()
    {
        return _pattern.AddFrame(_newFrame);
    }

    [Benchmark]
    public AnimationPattern RemoveFrame()
    {
        return _pattern.RemoveFrame(50);
    }

    [Benchmark]
    public AnimationPattern UpdateFrame()
    {
        return _pattern.UpdateFrame(50, _newFrame);
    }

    [Benchmark]
    public AnimationPattern MoveFrame()
    {
        return _pattern.MoveFrame(10, 80);
    }
}
