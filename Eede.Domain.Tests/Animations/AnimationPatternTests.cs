using Eede.Domain.Animations;
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
        var pattern = new AnimationPattern("Run", frames);

        Assert.Multiple(() =>
        {
            Assert.That(pattern.Name, Is.EqualTo("Run"));
            Assert.That(pattern.Frames.Count, Is.EqualTo(2));
            Assert.That(pattern.Frames[0].CellIndex, Is.EqualTo(0));
            Assert.That(pattern.Frames[0].Duration, Is.EqualTo(100));
        });
    }

    [Test]
    public void ImmutabilityTest()
    {
        var frames = new List<AnimationFrame>
        {
            new AnimationFrame(0, 100)
        };
        var pattern = new AnimationPattern("Walk", frames);
        
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
        var pattern = new AnimationPattern("Run", frames);
        
        var json = System.Text.Json.JsonSerializer.Serialize(pattern);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<AnimationPattern>(json);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.Name, Is.EqualTo("Run"));
            Assert.That(deserialized.Frames.Count, Is.EqualTo(2));
            Assert.That(deserialized.Frames[0].CellIndex, Is.EqualTo(0));
            Assert.That(deserialized.Frames[1].Duration, Is.EqualTo(200));
        });
    }
}
