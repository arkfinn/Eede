using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Domain.ImageEditing.Blending;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Eede.Domain.Tests.ImageEditing;

[TestFixture]
public class DrawingSessionBenchmark
{
    [Test]
    public void BenchmarkUndoRedoDiffs()
    {
        int width = 100;
        int height = 100;
        var initialPixels = new byte[width * height * 4];
        var initialPicture = Picture.Create(new PictureSize(width, height), initialPixels);
        var session = new DrawingSession(initialPicture);

        int diffCount = 100;
        var diffs = new List<PictureDiff>();
        Random rand = new Random(42);

        for (int i = 0; i < diffCount; i++)
        {
            int x = rand.Next(0, width - 10);
            int y = rand.Next(0, height - 10);
            var area = new PictureArea(new Position(x, y), new PictureSize(10, 10));
            var before = Picture.CreateEmpty(area.Size);
            var after = Picture.CreateEmpty(area.Size);
            diffs.Add(new PictureDiff(area, before, after));
        }

        var region = new PictureRegion(diffs.Select(d => d.Area));
        var nextPicture = initialPicture;
        var sessionWithDiffs = session.PushDiff(nextPicture, region);

        int iterations = 1;

        Stopwatch sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < iterations; i++)
        {
            var result = sessionWithDiffs.Undo();
            var redoResult = result.Session.Redo();
        }
        sw.Stop();

        Console.WriteLine($"Total time for {iterations} Undo/Redo with {diffCount} diffs: {sw.ElapsedMilliseconds}ms");
    }
}
