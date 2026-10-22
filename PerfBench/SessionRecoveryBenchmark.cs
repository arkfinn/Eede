using BenchmarkDotNet.Attributes;
using Eede.Application.Recovery;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Recovery;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Pictures;
using System;
using System.Collections.Generic;

namespace PerfBench;

[MemoryDiagnoser]
public class SessionRecoveryBenchmark
{
    private SkiaSharpPictureCodec _codec = default!;
    private SessionCapture _capture = default!;

    [Params(1, 2, 4, 8)]
    public int DocumentCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _codec = new SkiaSharpPictureCodec();
        var dict = new Dictionary<string, Picture>();
        var docs = new List<DocumentSnapshot>();

        for (int i = 0; i < DocumentCount; i++)
        {
            var id = $"doc_{i}";
            byte[] pixels = new byte[256 * 256 * 4];
            for (int p = 0; p < pixels.Length; p++)
            {
                pixels[p] = (byte)((p * 31 + i * 17) % 256);
            }
            var pic = Picture.Create(new PictureSize(256, 256), pixels);
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
    public IReadOnlyDictionary<string, byte[]> EncodeSequential()
    {
        var dict = new Dictionary<string, byte[]>(_capture.Pictures.Count);
        foreach (var (key, picture) in _capture.Pictures)
        {
            dict[key] = _codec.EncodeToPng(picture);
        }
        return dict;
    }

    [Benchmark]
    public IReadOnlyDictionary<string, byte[]> EncodeProductionLogic()
    {
        return SessionRecoveryCoordinator.EncodePayloads(_capture.Pictures, _codec);
    }
}
