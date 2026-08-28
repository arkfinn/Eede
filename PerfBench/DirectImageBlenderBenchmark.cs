using BenchmarkDotNet.Attributes;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System;

namespace PerfBench;

[MemoryDiagnoser]
public class DirectImageBlenderBenchmark
{
    private Picture _srcPic = null!;
    private Picture _destPic = null!;
    private DirectImageBlender _blender = null!;

    [GlobalSetup]
    public void Setup()
    {
        // 1024x1024 image
        int size = 1024;
        byte[] srcData = new byte[size * size * 4];
        byte[] destData = new byte[size * size * 4];

        Random r = new Random(42);
        r.NextBytes(srcData);
        r.NextBytes(destData);

        _srcPic = Picture.Create(new PictureSize(size, size), srcData);
        _destPic = Picture.Create(new PictureSize(size, size), destData);

        _blender = new DirectImageBlender();
    }

    [Benchmark(Baseline = true)]
    public Picture Blend()
    {
        return _blender.Blend(_srcPic, _destPic, new Position(0, 0));
    }
}
