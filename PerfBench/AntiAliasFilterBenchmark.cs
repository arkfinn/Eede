using BenchmarkDotNet.Attributes;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Filters;
using System;

namespace PerfBench
{
    [MemoryDiagnoser]
    public class AntiAliasFilterBenchmark
    {
        private Picture _source;
        private AntiAliasFilter _filter;

        [GlobalSetup]
        public void Setup()
        {
            int width = 800;
            int height = 600;
            byte[] data = new byte[width * height * 4];
            Random rnd = new Random(42);
            rnd.NextBytes(data);

            // Create some edges
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x % 50 < 25 && y % 50 < 25)
                    {
                        data[(y * width + x) * 4] = 255;
                        data[(y * width + x) * 4 + 1] = 255;
                        data[(y * width + x) * 4 + 2] = 255;
                    }
                    else
                    {
                        data[(y * width + x) * 4] = 0;
                        data[(y * width + x) * 4 + 1] = 0;
                        data[(y * width + x) * 4 + 2] = 0;
                    }
                    data[(y * width + x) * 4 + 3] = 255;
                }
            }

            _source = Picture.Create(new Eede.Domain.SharedKernel.PictureSize(width, height), data);
            _filter = new AntiAliasFilter(new ArgbAntiAliasStrategy(), 1);
        }

        [Benchmark(Baseline = true)]
        public Picture BenchmarkApply()
        {
            return _filter.Apply(_source);
        }
    }
}
