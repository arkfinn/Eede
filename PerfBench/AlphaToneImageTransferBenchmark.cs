using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.SharedKernel;
using Eede.Domain.ImageEditing;

namespace PerfBench
{
    [MemoryDiagnoser]
    public class AlphaToneImageTransferBenchmark
    {
        private Picture srcPic;
        private AlphaToneImageTransfer transfer;
        private Magnification magnification;

        [GlobalSetup]
        public void Setup()
        {
            int width = 800;
            int height = 600;
            byte[] fromPixels = new byte[width * height * 4];

            for (int i = 0; i < fromPixels.Length; i++)
            {
                fromPixels[i] = (byte)(i % 256);
            }

            srcPic = Picture.Create(new PictureSize(width, height), fromPixels);
            transfer = new AlphaToneImageTransfer();
            magnification = new Magnification(2.0f);
        }

        [Benchmark]
        public Picture Transfer()
        {
            return transfer.Transfer(srcPic, magnification);
        }
    }
}
