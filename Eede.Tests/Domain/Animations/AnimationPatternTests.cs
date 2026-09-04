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
    public void ValidateTest()
    {
        var validGrid = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        var validFrames = new List<AnimationFrame> { new AnimationFrame(0, 100) };

        var validPattern = new AnimationPattern("Valid", validFrames, validGrid);
        Assert.That(validPattern.Validate(), Is.True);

        var emptyNamePattern = new AnimationPattern("", validFrames, validGrid);
        Assert.That(emptyNamePattern.Validate(), Is.False);

        var tooLongNamePattern = new AnimationPattern(new string('a', 101), validFrames, validGrid);
        Assert.That(tooLongNamePattern.Validate(), Is.False);

        var emptyFramesPattern = new AnimationPattern("EmptyFrames", new List<AnimationFrame>(), validGrid);
        Assert.That(emptyFramesPattern.Validate(), Is.False, "Empty frames list must be invalid.");

        var zeroDurationPattern = new AnimationPattern("ZeroDuration", new List<AnimationFrame> { new AnimationFrame(0, 0) }, validGrid);
        Assert.That(zeroDurationPattern.Validate(), Is.False, "Duration == 0 must be invalid.");

        var invalidGridPattern = new AnimationPattern("InvalidGrid", validFrames, new GridSettings(new PictureSize(0, 32), new Position(0, 0), 0));
        Assert.That(invalidGridPattern.Validate(), Is.False);

        var negativeDurationPattern = new AnimationPattern("InvalidFrame", new List<AnimationFrame> { new AnimationFrame(0, -10) }, validGrid);
        Assert.That(negativeDurationPattern.Validate(), Is.False);

        var negativeIndexPattern = new AnimationPattern("InvalidIndex", new List<AnimationFrame> { new AnimationFrame(-1, 100) }, validGrid);
        Assert.That(negativeIndexPattern.Validate(), Is.False);
    }
}
