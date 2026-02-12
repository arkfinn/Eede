using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eede.Domain.Tests.Animations;

[TestFixture]
public class AnimationPatternsTests
{
    [Test]
    public void AddTest()
    {
        var patterns = new AnimationPatterns();
        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var pattern = new AnimationPattern("Test", new List<AnimationFrame>(), grid);

        var nextPatterns = patterns.Add(pattern);

        Assert.That(patterns.Items.Count, Is.EqualTo(0), "Original should not change");
        Assert.That(nextPatterns.Items.Count, Is.EqualTo(1));
        Assert.That(nextPatterns.Items[0], Is.EqualTo(pattern));
    }

    [Test]
    public void ReplaceTest()
    {
        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var pattern1 = new AnimationPattern("Test1", new List<AnimationFrame>(), grid);
        var pattern2 = new AnimationPattern("Test2", new List<AnimationFrame>(), grid);

        var patterns = new AnimationPatterns().Add(pattern1);
        var nextPatterns = patterns.Replace(0, pattern2);

        Assert.That(patterns.Items[0].Name, Is.EqualTo("Test1"));
        Assert.That(nextPatterns.Items[0].Name, Is.EqualTo("Test2"));
    }

    [Test]
    public void RemoveAtTest()
    {
        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var pattern = new AnimationPattern("Test", new List<AnimationFrame>(), grid);

        var patterns = new AnimationPatterns().Add(pattern);
        var nextPatterns = patterns.RemoveAt(0);

        Assert.That(patterns.Items.Count, Is.EqualTo(1));
        Assert.That(nextPatterns.Items.Count, Is.EqualTo(0));
    }

    [Test]
    public void InvalidArgumentsTest()
    {
        var patterns = new AnimationPatterns();
        Assert.Throws<ArgumentNullException>(() => patterns.Add(null));

        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var pattern = new AnimationPattern("Test", new List<AnimationFrame>(), grid);
        var patternsWithOne = patterns.Add(pattern);

        Assert.Throws<ArgumentOutOfRangeException>(() => patternsWithOne.Replace(-1, pattern));
        Assert.Throws<ArgumentOutOfRangeException>(() => patternsWithOne.Replace(1, pattern));
        Assert.Throws<ArgumentNullException>(() => patternsWithOne.Replace(0, null));

        Assert.Throws<ArgumentOutOfRangeException>(() => patternsWithOne.RemoveAt(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => patternsWithOne.RemoveAt(1));
    }
}
