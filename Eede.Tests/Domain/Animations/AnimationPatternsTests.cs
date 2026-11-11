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
        Assert.Throws<ArgumentNullException>(() => patterns.Add(null!));

        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var pattern = new AnimationPattern("Test", new List<AnimationFrame>(), grid);

        var ex0 = Assert.Throws<ArgumentOutOfRangeException>(() => patterns.Replace(0, pattern));
        Assert.That(ex0.ParamName, Is.EqualTo("index"));
        Assert.That(ex0.Message, Does.Contain("Index is out of range."));

        var exNeg = Assert.Throws<ArgumentOutOfRangeException>(() => patterns.Replace(-1, pattern));
        Assert.That(exNeg.ParamName, Is.EqualTo("index"));
        Assert.That(exNeg.Message, Does.Contain("Index is out of range."));

        var patternsWithOne = patterns.Add(pattern);

        var exNeg1 = Assert.Throws<ArgumentOutOfRangeException>(() => patternsWithOne.Replace(-1, pattern));
        Assert.That(exNeg1.ParamName, Is.EqualTo("index"));
        Assert.That(exNeg1.Message, Does.Contain("Index is out of range."));

        var ex1 = Assert.Throws<ArgumentOutOfRangeException>(() => patternsWithOne.Replace(1, pattern));
        Assert.That(ex1.ParamName, Is.EqualTo("index"));
        Assert.That(ex1.Message, Does.Contain("Index is out of range."));

        Assert.Throws<ArgumentNullException>(() => patternsWithOne.Replace(0, null!));

        var exRemNeg = Assert.Throws<ArgumentOutOfRangeException>(() => patternsWithOne.RemoveAt(-1));
        Assert.That(exRemNeg.ParamName, Is.EqualTo("index"));
        Assert.That(exRemNeg.Message, Does.Contain("Index is out of range."));

        var exRem1 = Assert.Throws<ArgumentOutOfRangeException>(() => patternsWithOne.RemoveAt(1));
        Assert.That(exRem1.ParamName, Is.EqualTo("index"));
        Assert.That(exRem1.Message, Does.Contain("Index is out of range."));

        var exRem0 = Assert.Throws<ArgumentOutOfRangeException>(() => patterns.RemoveAt(0));
        Assert.That(exRem0.ParamName, Is.EqualTo("index"));
        Assert.That(exRem0.Message, Does.Contain("Index is out of range."));
    }
}
