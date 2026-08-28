using BenchmarkDotNet.Attributes;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Domain.ImageEditing.History;
using System.Collections.Generic;

namespace PerfBench
{
    [MemoryDiagnoser]
    public class DrawingSessionBenchmark
    {
        private DrawingSession _sessionUndo;
        private DrawingSession _sessionRedo;

        [GlobalSetup]
        public void Setup()
        {
            var width = 256;
            var height = 256;
            var bytes1 = new byte[width * height * 4];
            var bytes2 = new byte[width * height * 4];

            for(int i = 0; i < bytes2.Length; i++) {
                bytes2[i] = 255;
            }

            var size = new PictureSize(width, height);
            var pic1 = Picture.Create(size, bytes1);
            var pic2 = Picture.Create(size, bytes2);

            var session = new DrawingSession(pic1);

            var areas = new List<PictureArea>();
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    areas.Add(new PictureArea(new Position(x, y), new PictureSize(4, 4)));
                }
            }
            var region = new PictureRegion(areas);

            _sessionUndo = session.PushDiff(pic2, region);
            _sessionRedo = _sessionUndo.Undo().Session;
        }

        [Benchmark]
        public UndoResult BenchmarkUndo()
        {
            return _sessionUndo.Undo();
        }

        [Benchmark]
        public RedoResult BenchmarkRedo()
        {
            return _sessionRedo.Redo();
        }
    }
}
