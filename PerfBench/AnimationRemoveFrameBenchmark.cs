using BenchmarkDotNet.Attributes;
using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using System.Collections.Generic;
using System.Linq;

namespace PerfBench;

[MemoryDiagnoser]
public class AnimationRemoveFrameBenchmark
{
    private AnimationPattern _pattern;
    private AnimationFrame _targetFrame;

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
        // target the middle frame to represent average case
        _targetFrame = frames[50];
    }

    [Benchmark(Baseline = true)]
    public int BaselineIndexOf()
    {
        var frames = _pattern.Frames.ToList();
        return frames.IndexOf(_targetFrame);
    }

    [Benchmark]
    public int OptimizedIndexOfLoop()
    {
        var frames = _pattern.Frames;
        for (int i = 0; i < frames.Count; i++)
        {
            if (frames[i] == _targetFrame)
            {
                return i;
            }
        }
        return -1;
    }

    [Benchmark]
    public int OptimizedIndexOfCasting()
    {
        var frames = _pattern.Frames;
        if (frames is AnimationFrame[] array)
        {
            return System.Array.IndexOf(array, _targetFrame);
        }
        else if (frames is List<AnimationFrame> list)
        {
            return list.IndexOf(_targetFrame);
        }
        else
        {
            for (int i = 0; i < frames.Count; i++)
            {
                if (frames[i] == _targetFrame)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
