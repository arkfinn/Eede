using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Eede.Domain.Tests.Animations;

[TestFixture]
public class AnimationPatternTests
{
    [Test]
    public void ConstructorTest()
    {
        var frames = new List<AnimationFrame>
        {
            new AnimationFrame(0, 100),
            new AnimationFrame(1, 200)
        };
        var gridSize = new PictureSize(32, 32);
        var offset = new Position(0, 0);
        var grid = new GridSettings(gridSize, offset, 0);
        var pattern = new AnimationPattern("Run", frames, grid);

        Assert.Multiple(() =>
        {
            Assert.That(pattern.Name, Is.EqualTo("Run"));
            Assert.That(pattern.Frames.Count, Is.EqualTo(2));
            Assert.That(pattern.Frames[0].CellIndex, Is.EqualTo(0));
            Assert.That(pattern.Frames[0].Duration, Is.EqualTo(100));
            Assert.That(pattern.Grid, Is.EqualTo(grid));
        });
    }

    [Test]
    public void ImmutabilityTest()
    {
        var frames = new List<AnimationFrame>
        {
            new AnimationFrame(0, 100)
        };
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var pattern = new AnimationPattern("Walk", frames, grid);

        // Ensure we cannot modify the internal list through the reference passed to constructor
        frames.Add(new AnimationFrame(1, 100));

        Assert.That(pattern.Frames.Count, Is.EqualTo(1));
    }

    [Test]
    public void SerializationTest()
    {
        var frames = new List<AnimationFrame>
        {
            new AnimationFrame(0, 100),
            new AnimationFrame(1, 200)
        };
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var pattern = new AnimationPattern("Run", frames, grid);

        var json = System.Text.Json.JsonSerializer.Serialize(pattern);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<AnimationPattern>(json);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.Name, Is.EqualTo("Run"));
            Assert.That(deserialized.Frames.Count, Is.EqualTo(2));
            Assert.That(deserialized.Frames[0].CellIndex, Is.EqualTo(0));
            Assert.That(deserialized.Frames[1].Duration, Is.EqualTo(200));
            Assert.That(deserialized.Grid, Is.EqualTo(grid));
        });
    }

    [Test]
    public void AddFrameTest()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var pattern = new AnimationPattern("Test", new List<AnimationFrame>(), grid);
        var frame = new AnimationFrame(0, 100);

        var newPattern = pattern.AddFrame(frame);

        Assert.That(newPattern.Frames.Count, Is.EqualTo(1));
        Assert.That(newPattern.Frames[0], Is.EqualTo(frame));
        Assert.That(newPattern.Grid, Is.EqualTo(grid));
    }

    [Test]
    public void RemoveFrameTest()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var frames = new List<AnimationFrame> { new AnimationFrame(0, 100), new AnimationFrame(1, 100) };
        var pattern = new AnimationPattern("Test", frames, grid);

        var newPattern = pattern.RemoveFrame(0);

        Assert.That(newPattern.Frames.Count, Is.EqualTo(1));
        Assert.That(newPattern.Frames[0].CellIndex, Is.EqualTo(1));
    }

    [Test]
    public void MoveFrameTest()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var frames = new List<AnimationFrame> {
            new AnimationFrame(0, 100),
            new AnimationFrame(1, 100),
            new AnimationFrame(2, 100)
        };
        var pattern = new AnimationPattern("Test", frames, grid);

        // Move index 0 to index 2 (end)
        var newPattern = pattern.MoveFrame(0, 2);

        Assert.That(newPattern.Frames[0].CellIndex, Is.EqualTo(1));
        Assert.That(newPattern.Frames[1].CellIndex, Is.EqualTo(2));
        Assert.That(newPattern.Frames[2].CellIndex, Is.EqualTo(0));
    }

    [Test]
    public void UpdateFrameTest()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var frames = new List<AnimationFrame> { new AnimationFrame(0, 100) };
        var pattern = new AnimationPattern("Test", frames, grid);
        var newFrame = new AnimationFrame(0, 200);

        var newPattern = pattern.UpdateFrame(0, newFrame);

        Assert.That(newPattern.Frames[0].Duration, Is.EqualTo(200));
    }

    [Test]
    public void RemoveFrameWithInvalidIndexTest()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var frames = new List<AnimationFrame> { new AnimationFrame(0, 100) };
        var pattern = new AnimationPattern("Test", frames, grid);

        Assert.Throws<System.ArgumentOutOfRangeException>(() => pattern.RemoveFrame(1));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => pattern.RemoveFrame(-1));
    }

    [Test]
    public void MoveFrameWithInvalidIndexTest()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var frames = new List<AnimationFrame> { new AnimationFrame(0, 100) };
        var pattern = new AnimationPattern("Test", frames, grid);

        Assert.Throws<System.ArgumentOutOfRangeException>(() => pattern.MoveFrame(0, 1));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => pattern.MoveFrame(0, 2));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => pattern.MoveFrame(0, -1));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => pattern.MoveFrame(1, 0));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => pattern.MoveFrame(2, 0));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => pattern.MoveFrame(-1, 0));
    }

    [Test]
    public void UpdateFrameWithInvalidIndexTest()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var frames = new List<AnimationFrame> { new AnimationFrame(0, 100) };
        var pattern = new AnimationPattern("Test", frames, grid);

        Assert.Throws<System.ArgumentOutOfRangeException>(() => pattern.UpdateFrame(1, new AnimationFrame(0, 200)));
    }

    [Test]
    public void Constructor_WithInvalidValues_ThrowsException()
    {
        var validGrid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var validFrames = new List<AnimationFrame> { new AnimationFrame(0, 100) };

        Assert.Throws<System.ArgumentException>(() => new AnimationPattern("", validFrames, validGrid));
        Assert.Throws<System.ArgumentException>(() => new AnimationPattern(new string('a', 101), validFrames, validGrid));
        Assert.Throws<System.ArgumentNullException>(() => new AnimationPattern("Test", null!, validGrid));
        Assert.Throws<System.ArgumentNullException>(() => new AnimationPattern("Test", validFrames, null!));
    }

    [Test]
    public void ValidateTest()
    {
        var validGrid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var validFrames = new List<AnimationFrame> { new AnimationFrame(0, 100) };

        var validPattern = new AnimationPattern("Valid", validFrames, validGrid);
        Assert.That(validPattern.Validate(), Is.True);

        var emptyFramesPattern = new AnimationPattern("EmptyFrames", new List<AnimationFrame>(), validGrid);
        Assert.That(emptyFramesPattern.Validate(), Is.True, "Empty frames list is valid.");
    }

    [Test]
    public void MoveFrame_WhenSameIndex_ReturnsSameInstance()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var frames = new List<AnimationFrame> { new AnimationFrame(0, 100), new AnimationFrame(1, 100) };
        var pattern = new AnimationPattern("Test", frames, grid);

        var result = pattern.MoveFrame(1, 1);

        Assert.That(result, Is.SameAs(pattern));
    }

    [Test]
    public void MoveFrame_StartToEndAndEndToStart_MovesCorrectly()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var frames = new List<AnimationFrame>
        {
            new AnimationFrame(0, 100),
            new AnimationFrame(1, 100),
            new AnimationFrame(2, 100)
        };
        var pattern = new AnimationPattern("Test", frames, grid);

        // Move start to end: 0 -> 2
        var moveToEnd = pattern.MoveFrame(0, 2);
        Assert.Multiple(() =>
        {
            Assert.That(moveToEnd.Frames[0].CellIndex, Is.EqualTo(1));
            Assert.That(moveToEnd.Frames[1].CellIndex, Is.EqualTo(2));
            Assert.That(moveToEnd.Frames[2].CellIndex, Is.EqualTo(0));
        });

        // Move end to start: 2 -> 0
        var moveToStart = pattern.MoveFrame(2, 0);
        Assert.Multiple(() =>
        {
            Assert.That(moveToStart.Frames[0].CellIndex, Is.EqualTo(2));
            Assert.That(moveToStart.Frames[1].CellIndex, Is.EqualTo(0));
            Assert.That(moveToStart.Frames[2].CellIndex, Is.EqualTo(1));
        });
    }

    [Test]
    public void MoveFrame_MiddleElements_MovesCorrectly()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var frames = new List<AnimationFrame>
        {
            new AnimationFrame(0, 100),
            new AnimationFrame(1, 100),
            new AnimationFrame(2, 100),
            new AnimationFrame(3, 100)
        };
        var pattern = new AnimationPattern("Test", frames, grid);

        // Move middle elements: 1 -> 2
        var result1 = pattern.MoveFrame(1, 2);
        Assert.Multiple(() =>
        {
            Assert.That(result1.Frames[0].CellIndex, Is.EqualTo(0));
            Assert.That(result1.Frames[1].CellIndex, Is.EqualTo(2));
            Assert.That(result1.Frames[2].CellIndex, Is.EqualTo(1));
            Assert.That(result1.Frames[3].CellIndex, Is.EqualTo(3));
        });

        // Move middle elements: 2 -> 1
        var result2 = pattern.MoveFrame(2, 1);
        Assert.Multiple(() =>
        {
            Assert.That(result2.Frames[0].CellIndex, Is.EqualTo(0));
            Assert.That(result2.Frames[1].CellIndex, Is.EqualTo(2));
            Assert.That(result2.Frames[2].CellIndex, Is.EqualTo(1));
            Assert.That(result2.Frames[3].CellIndex, Is.EqualTo(3));
        });
    }

    [Test]
    public void RemoveFrame_StartMiddleEnd_RemovesCorrectly()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var frames = new List<AnimationFrame>
        {
            new AnimationFrame(0, 100),
            new AnimationFrame(1, 100),
            new AnimationFrame(2, 100)
        };
        var pattern = new AnimationPattern("Test", frames, grid);

        // Remove start
        var removeStart = pattern.RemoveFrame(0);
        Assert.That(removeStart.Frames.Select(f => f.CellIndex), Is.EqualTo(new[] { 1, 2 }));

        // Remove middle
        var removeMiddle = pattern.RemoveFrame(1);
        Assert.That(removeMiddle.Frames.Select(f => f.CellIndex), Is.EqualTo(new[] { 0, 2 }));

        // Remove end
        var removeEnd = pattern.RemoveFrame(2);
        Assert.That(removeEnd.Frames.Select(f => f.CellIndex), Is.EqualTo(new[] { 0, 1 }));
    }

    [Test]
    public void AddFrameAndUpdateFrame_ExceptionTests()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var pattern = new AnimationPattern("Test", new List<AnimationFrame> { new AnimationFrame(0, 100) }, grid);

        // Null frame throws ArgumentNullException
        Assert.Throws<System.ArgumentNullException>(() => pattern.AddFrame(null!));
        Assert.Throws<System.ArgumentNullException>(() => pattern.UpdateFrame(0, null!));

        // Invalid frame (e.g., duration <= 0) throws ArgumentException
        var invalidFrame = new AnimationFrame(0, 0);
        Assert.Throws<System.ArgumentException>(() => pattern.AddFrame(invalidFrame));
        Assert.Throws<System.ArgumentException>(() => pattern.UpdateFrame(0, invalidFrame));

        // Out of bounds index in UpdateFrame throws ArgumentOutOfRangeException
        var validFrame = new AnimationFrame(1, 100);
        Assert.Throws<System.ArgumentOutOfRangeException>(() => pattern.UpdateFrame(-1, validFrame));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => pattern.UpdateFrame(1, validFrame));
    }

    [Test]
    public void Constructor_EdgeCases_NullAndEmptyCollections()
    {
        var grid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);

        // Null frames throws ArgumentNullException
        Assert.Throws<System.ArgumentNullException>(() => new AnimationPattern("Test", (IReadOnlyList<AnimationFrame>)null!, grid));
        Assert.Throws<System.ArgumentNullException>(() => new AnimationPattern("Test", (IEnumerable<AnimationFrame>)null!, grid));

        // Empty collection initializes correctly
        var emptyPattern1 = new AnimationPattern("Empty1", new List<AnimationFrame>(), grid);
        Assert.That(emptyPattern1.Frames.Count, Is.EqualTo(0));

        var emptyPattern2 = new AnimationPattern("Empty2", Enumerable.Empty<AnimationFrame>(), grid);
        Assert.That(emptyPattern2.Frames.Count, Is.EqualTo(0));
    }
}
