using BenchmarkDotNet.Attributes;
using Eede.Application.Palettes;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Infrastructure.Palettes;
using System;

namespace PerfBench;

[MemoryDiagnoser]
public class PaletteQuantizationBenchmark
{
    private Picture _picture2000x2000 = default!;
    private Picture _picture512x512 = default!;

    [GlobalSetup]
    public void Setup()
    {
        _picture2000x2000 = CreateBenchmarkPicture(2000, 2000);
        _picture512x512 = CreateBenchmarkPicture(512, 512);
    }

    private static Picture CreateBenchmarkPicture(int width, int height)
    {
        byte[] pixels = new byte[width * height * 4];
        for (int y = 0; y < height; y++)
        {
            byte r = (byte)(y & 0xFF);
            byte g = (byte)((y >> 2) & 0xFF);
            int rowOffset = y * width * 4;
            for (int x = 0; x < width; x++)
            {
                int idx = rowOffset + (x * 4);
                pixels[idx + 0] = (byte)((x >> 3) & 0xFF); // B (8px単位のラン)
                pixels[idx + 1] = g;                       // G
                pixels[idx + 2] = r;                       // R
                pixels[idx + 3] = 255;                     // A
            }
        }
        return Picture.Create(new PictureSize(width, height), pixels);
    }

    [Benchmark]
    public PaletteExtractionResult? ScanFullColor512x512()
    {
        return DirectColorPaletteScanner.Scan(_picture512x512);
    }

    [Benchmark]
    public PaletteExtractionResult? ScanFullColor2000x2000()
    {
        return DirectColorPaletteScanner.Scan(_picture2000x2000);
    }
}
