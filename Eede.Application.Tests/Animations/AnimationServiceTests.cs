using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System.Collections.Generic;

namespace Eede.Application.Tests.Animations;

[TestFixture]
public class AnimationServiceTests
{
    [Test]
    public void AddAndRemoveTest()
    {
        var service = new AnimationService();
        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var pattern = new AnimationPattern("Test", new List<AnimationFrame>(), grid);
        
        service.Add(pattern);
        Assert.That(service.Patterns.Count, Is.EqualTo(1));
        
        service.Remove(0);
        Assert.That(service.Patterns.Count, Is.EqualTo(0));
    }

    [Test]
    public void ReplaceTest()
    {
        var service = new AnimationService();
        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var pattern1 = new AnimationPattern("Test1", new List<AnimationFrame>(), grid);
        var pattern2 = new AnimationPattern("Test2", new List<AnimationFrame>(), grid);
        
        service.Add(pattern1);
        service.Replace(0, pattern2);
        
        Assert.That(service.Patterns.Count, Is.EqualTo(1));
        Assert.That(service.Patterns[0].Name, Is.EqualTo("Test2"));
    }

    [Test]
    public void AddNullTest()
    {
        var service = new AnimationService();
        Assert.Throws<System.ArgumentNullException>(() => service.Add(null));
    }

    [Test]
    public void ReplaceInvalidTest()
    {
        var service = new AnimationService();
        var grid = new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0);
        var pattern = new AnimationPattern("Test", new List<AnimationFrame>(), grid);

        Assert.Throws<System.ArgumentOutOfRangeException>(() => service.Replace(0, pattern));
        
        service.Add(pattern);
        Assert.Throws<System.ArgumentNullException>(() => service.Replace(0, null));
    }

    [Test]
    public void RemoveInvalidTest()
    {
        var service = new AnimationService();
        Assert.Throws<System.ArgumentOutOfRangeException>(() => service.Remove(0));
    }
}
