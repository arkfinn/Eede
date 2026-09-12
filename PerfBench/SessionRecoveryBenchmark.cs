using BenchmarkDotNet.Attributes;
using Eede.Application.Recovery;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Recovery;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Pictures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PerfBench;

[MemoryDiagnoser]
public class SessionRecoveryBenchmark
{
    private SkiaSharpPictureCodec _codec = default!;
    private SessionCapture _capture = default!;

    [GlobalSetup]
    public void Setup()
    {
        _codec = new SkiaSharpPictureCodec();
        var dict = new Dictionary<string, Picture>();
        var docs = new List<DocumentSnapshot>();

        for (int i = 0; i < 8; i++)
        {
            var id = $"doc_{i}";
            var pic = Picture.CreateEmpty(new PictureSize(256, 256));
            dict[id] = pic;
            docs.Add(new DocumentSnapshot(id, null, true, pic.Size, 1.0f, id));
        }

        var snapshot = new SessionSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "doc_0",
            docs,
            null,
            new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, Array.Empty<ArgbColor>()));

        _capture = new SessionCapture(snapshot, dict);
    }

    [Benchmark(Baseline = true)]
    public Dictionary<string, byte[]> EncodeSequential()
    {
        var encodedPayloads = new Dictionary<string, byte[]>();
        foreach (var (key, picture) in _capture.Pictures)
        {
            var encoded = _codec.EncodeToPng(picture);
            encodedPayloads[key] = encoded;
        }
        return encodedPayloads;
    }

    [Benchmark]
    public ConcurrentDictionary<string, byte[]> EncodeParallel()
    {
        var encodedPayloads = new ConcurrentDictionary<string, byte[]>();
        Parallel.ForEach(_capture.Pictures, kvp =>
        {
            var encoded = _codec.EncodeToPng(kvp.Value);
            encodedPayloads[kvp.Key] = encoded;
        });
        return encodedPayloads;
    }
}
