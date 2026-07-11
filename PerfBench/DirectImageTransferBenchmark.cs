using BenchmarkDotNet.Attributes;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.SharedKernel;

namespace PerfBench
{
    [MemoryDiagnoser]
    public class DirectImageTransferBenchmark
    {
        private Picture fromPic;
        private DirectImageTransfer transfer;
        private Magnification mag1;
        private Magnification mag2;

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

            fromPic = Picture.Create(new PictureSize(width, height), fromPixels);
            transfer = new DirectImageTransfer();
            mag1 = new Magnification(1.0f);
            mag2 = new Magnification(2.0f);
        }

        [Benchmark]
        public Picture Transfer1x()
        {
            return transfer.Transfer(fromPic, mag1);
        }

        [Benchmark]
        public Picture Transfer2x()
        {
            return transfer.Transfer(fromPic, mag2);
        }
    }
}
