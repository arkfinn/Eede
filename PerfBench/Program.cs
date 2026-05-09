using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.SharedKernel;
using Eede.Domain.ImageEditing;

namespace PerfBench
{
    [MemoryDiagnoser]
    public class AlphaOnlyImageBlenderBenchmark
    {
        private Picture fromPic;
        private Picture toPic;
        private AlphaOnlyImageBlender blender;

        [GlobalSetup]
        public void Setup()
        {
            // Create some images (e.g., 800x600)
            int width = 800;
            int height = 600;
            byte[] fromPixels = new byte[width * height * 4];
            byte[] toPixels = new byte[width * height * 4];

            // Just fill with some dummy data
            for (int i = 0; i < fromPixels.Length; i++)
            {
                fromPixels[i] = (byte)(i % 256);
                toPixels[i] = (byte)(255 - (i % 256));
            }

            fromPic = Picture.Create(new PictureSize(width, height), fromPixels);
            toPic = Picture.Create(new PictureSize(width, height), toPixels);
            blender = new AlphaOnlyImageBlender();
        }

        [Benchmark]
        public Picture BlendAlphaOnly()
        {
            return blender.Blend(fromPic, toPic);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<AlphaOnlyImageBlenderBenchmark>();
        }
    }
}
