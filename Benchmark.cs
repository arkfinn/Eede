using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using System.Collections.Generic;

[MemoryDiagnoser]
public class AnimationPatternBenchmark
{
    private GridSettings _grid;
    private List<AnimationFrame> _frames;
    private IReadOnlyList<AnimationFrame> _readOnlyFrames;

    [GlobalSetup]
    public void Setup()
    {
        _grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        _frames = new List<AnimationFrame>();
        for (int i = 0; i < 100; i++)
        {
            _frames.Add(new AnimationFrame(i, 100));
        }
        _readOnlyFrames = _frames;
    }

    [Benchmark(Baseline = true)]
    public AnimationPattern BaselineConstructor()
    {
        return new AnimationPattern("Test", _readOnlyFrames, _grid);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<AnimationPatternBenchmark>();
    }
}
